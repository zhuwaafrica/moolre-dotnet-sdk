"""Generates docs/reference.md from the SDK's XML documentation and its public API list, so the reference always matches the code.

Usage (from the repository root, after a Release build):
    dotnet build src/Zhuwa.Moolre.Sdk -c Release
    python3 eng/generate-reference.py .
"""
import re, sys, glob, xml.etree.ElementTree as ET
root = sys.argv[1]
xmlp = f'{root}/artifacts/bin/Zhuwa.Moolre.Sdk/release/Zhuwa.Moolre.Sdk.xml'
api = open(f'{root}/src/Zhuwa.Moolre.Sdk/.PublicAPI/PublicAPI.Unshipped.txt').read().splitlines()
docs = {m.get('name'): m for m in ET.parse(xmlp).getroot().iter('member')}

def short(t):
    t = t.replace('!', '')
    t = re.sub(r'default\([^)]*\)', 'default', t)
    t = re.sub(r'\b(?:[A-Za-z_][\w]*\.)+([A-Za-z_]\w*)', r'\1', t)
    return t

def text(el):
    if el is None: return ''
    out = [el.text or '']
    for c in el:
        tag = c.tag
        if tag == 'see' or tag == 'seealso':
            if c.get('langword'): out.append(f'`{c.get("langword")}`')
            else:
                cref = c.get('cref', '')
                name = re.sub(r'\(.*\)', '', cref.split(':', 1)[-1]).split('.')[-1]
                if '{' in name: name = name.split('{')[0]
                out.append(f'`{name}`')
        elif tag == 'paramref' or tag == 'typeparamref': out.append(f'`{c.get("name")}`')
        elif tag == 'c': out.append(f'`{text(c)}`')
        elif tag == 'b': out.append(f'**{text(c)}**')
        else: out.append(text(c))
        out.append(c.tail or '')
    return re.sub(r'\s+', ' ', ''.join(out)).strip()

def xml_params(member_name):
    m = re.search(r'\((.*)\)$', member_name)
    if not m: return []
    depth = 0; cur = ''; parts = []
    for ch in m.group(1):
        if ch in '{(': depth += 1
        if ch in '})': depth -= 1
        if ch == ',' and depth == 0: parts.append(cur); cur = ''
        else: cur += ch
    if cur: parts.append(cur)
    return [re.sub(r'\{.*\}', '', p).rstrip('@').split('.')[-1] for p in parts]

def find_doc(typ, method, param_types):
    for name, el in docs.items():
        if not name.startswith(f'M:{typ}.{method}'): continue
        rest = name[len(f'M:{typ}.{method}'):]
        if rest and not rest.startswith('('): continue
        if xml_params(name) == param_types: return el
    return None

def api_methods(typ):
    rows = []
    for line in api:
        m = re.match(r'^(?:virtual |abstract |static )*' + re.escape(typ) + r'\.(\w+)\((.*)\) -> (.+)$', line)
        if not m or m.group(1) == typ.split('.')[-1]: continue
        name, params, ret = m.groups()
        ptypes = []
        for p in re.split(r', (?![^<]*>)', params) if params else []:
            p = re.sub(r'^(this|out|ref|in) ', '', p)
            t = p.split(' ')[0].replace('!', '').replace('?', '')
            t = {'string':'String','int':'Int32','bool':'Boolean','decimal':'Decimal','byte':'Byte'}.get(t, t)
            ptypes.append(re.sub(r'<.*>', '', t).split('.')[-1])
        rows.append((name, short(params), short(ret), ptypes, line.startswith('static')))
    return rows

def request_fields(req_type):
    files = glob.glob(f'{root}/src/Zhuwa.Moolre.Sdk/**/{req_type}.cs', recursive=True)
    if not files: return []
    src = open(files[0]).read()
    ns = re.search(r'^namespace ([\w.]+);', src, re.M).group(1)
    out = []
    for m in re.finditer(r'^    public (required )?([\w<>?,. ]+?) (\w+) \{', src, re.M):
        req, typ, prop = m.groups()
        d = docs.get(f'P:{ns}.{req_type}.{prop}')
        out.append((prop, short(typ), bool(req), text(d.find('summary')) if d is not None else ''))
    return out

areas = [
 ('Payments','Zhuwa.Moolre.Payments.MoolrePayments','moolre.Payments','Uses the public key.'),
 ('Transfers','Zhuwa.Moolre.Transfers.MoolreTransfers','moolre.Transfers','Sending uses the private key; the name check and status accept either key.'),
 ('Accounts','Zhuwa.Moolre.Accounts.MoolreAccounts','moolre.Accounts','Uses the private key.'),
 ('Sales','Zhuwa.Moolre.Sales.MoolreSales','moolre.Sales','Uses the private key. The wallet must be linked to Moolre\'s Sell on Our Platform service.'),
 ('SMS','Zhuwa.Moolre.Sms.MoolreSms','moolre.Sms','Uses only the SMS VAS key.'),
 ('WhatsApp','Zhuwa.Moolre.WhatsApp.MoolreWhatsApp','moolre.WhatsApp','Uses only the WhatsApp VAS key.'),
 ('Reference data','Zhuwa.Moolre.Reference.MoolreReferenceData','moolre.ReferenceData','Needs no credentials.'),
]
L = ['# API reference', '',
     'Every method in the SDK, with its signature, what it does, its parameters and the exceptions it can throw. This page is generated from the SDK\'s own documentation comments, so it always matches the code; see the [user guides](README.md) for worked examples.', '',
     '**Areas:** ' + ' · '.join(f'[{a[0]}](#{a[0].lower().replace(" ", "-")})' for a in areas) + ' · [Setup](#setup) · [Webhooks](#webhooks)', '']

def emit_method(typ, prefix, name, params, ret, ptypes, static=False):
    d = find_doc(typ, name, ptypes)
    call = f'{typ.split(".")[-1]}.{name}' if static else f'{prefix}.{name}'
    L.append(f'### `{name}`'); L.append('')
    L.append('```csharp'); L.append(f'{ret} {call}({params})'); L.append('```'); L.append('')
    if d is None: return
    s = text(d.find('summary'))
    if s: L.append(s); L.append('')
    ps = [(p.get('name'), text(p)) for p in d.findall('param')]
    # request type fields
    req = next((t for t in ptypes if t.endswith('Request')), None)
    if ps:
        L.append('| Parameter | Description |'); L.append('| --- | --- |')
        for n, t in ps: L.append(f'| `{n}` | {t} |')
        L.append('')
    if req:
        fields = request_fields(req)
        if fields:
            L.append(f'<details><summary><code>{req}</code> fields</summary>'); L.append('')
            L.append('| Property | Type | Required | Description |'); L.append('| --- | --- | --- | --- |')
            for prop, t, r, desc in fields: L.append(f'| `{prop}` | `{t}` | {"yes" if r else ""} | {desc} |')
            L.append(''); L.append('</details>'); L.append('')
    r = text(d.find('returns'))
    if r: L.append(f'**Returns:** {r}'); L.append('')
    ex = [(re.sub(r'.*\.', '', e.get('cref','')), text(e)) for e in d.findall('exception')]
    if ex:
        L.append('| Throws | When |'); L.append('| --- | --- |')
        for n, t in ex: L.append(f'| `{n}` | {t} |')
        L.append('')

seen_sigs=set()
for title, typ, prefix, note in areas:
    L.append(f'## {title}'); L.append(''); L.append(f'`{prefix}`. {note}'); L.append('')
    for name, params, ret, ptypes, static in api_methods(typ):
        emit_method(typ, prefix, name, params, ret, ptypes)

L.append('## Setup'); L.append('')
for typ, prefix in [('Zhuwa.Moolre.DependencyInjection.MoolreServiceCollectionExtensions','services'), ('Zhuwa.Moolre.MoolreClientFactory','factory')]:
    for name, params, ret, ptypes, static in api_methods(typ):
        if static and typ.endswith('Extensions'):
            params = params.replace('this IServiceCollection services, ', '')
            emit_method(typ, 'services', name, params, ret, ptypes[1:] and ['IServiceCollection'] + ptypes[1:])
        else:
            emit_method(typ, prefix, name, params, ret, ptypes)
L.append('## Webhooks'); L.append('')
for typ, prefix in [('Zhuwa.Moolre.PaymentWebhook','PaymentWebhook'), ('Zhuwa.Moolre.Webhooks.MoolreWebhookVerifier','verifier')]:
    for name, params, ret, ptypes, static in api_methods(typ):
        if name in ('Equals','GetHashCode','ToString'): continue
        emit_method(typ, prefix, name, params, ret, ptypes, static)

open(f'{root}/docs/reference.md','w').write('\n'.join(L).rstrip()+'\n')
print('methods:', sum(1 for l in L if l.startswith('### ')))
