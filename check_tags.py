
import os
import re

def check_razor_tags(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Simple check for balanced <div> and </div>
    open_divs = len(re.findall(r'<div\b', content))
    close_divs = len(re.findall(r'</div>', content))
    
    if open_divs != close_divs:
        print(f"File: {file_path}")
        print(f"  Open divs: {open_divs}")
        print(f"  Close divs: {close_divs}")
        print(f"  Difference: {open_divs - close_divs}")

base_path = r'c:\Users\User\OneDrive\Escritorio\SplitMoney\SplitMoney.Client\Components\Pages'
for root, dirs, files in os.walk(base_path):
    for file in files:
        if file.endswith('.razor'):
            check_razor_tags(os.path.join(root, file))

base_path_dashboard = r'c:\Users\User\OneDrive\Escritorio\SplitMoney\SplitMoney.Client\Components\Dashboard'
for root, dirs, files in os.walk(base_path_dashboard):
    for file in files:
        if file.endswith('.razor'):
            check_razor_tags(os.path.join(root, file))
