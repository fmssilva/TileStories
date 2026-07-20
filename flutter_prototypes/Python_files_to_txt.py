import os
import pathlib

def collect_files_to_txt(root_dir):
    
    output_file = "project_files.txt"
    output_path = os.path.join(root_dir, output_file)
    
    with open(output_path, 'w', encoding='utf-8') as outfile:
        for item in os.listdir(root_dir):
            item_path = os.path.join(root_dir, item)
            if os.path.isdir(item_path) and item != "export":  # Skip the export folder itself
                for root, dirs, files in os.walk(item_path):
                    for file in files:
                        file_path = os.path.join(root, file)
                        # Skip .txt and .md files
                        if file.endswith(".txt") or file.endswith(".md"):
                            continue
                        try:
                            with open(file_path, 'r', encoding='utf-8') as infile:
                                content = infile.read()
                            outfile.write(f"\n\n\n\n\n\n\n")
                            outfile.write(f"==================== File: {file} ====================\n")
                            outfile.write(f"\nAbsolute path: {file_path}\n\n\n")
                            outfile.write(content)
                            outfile.write("\n\n" + "="*50 + "\n\n")
                        except Exception as e:
                            outfile.write(f"\n\n\n\n\n\n\nAbsolute path: {file_path}\n")
                            outfile.write(f"Error reading file: {e}\n\n" + "="*50 + "\n\n")

if __name__ == "__main__":
    current_dir = os.path.dirname(os.path.abspath(__file__))
    collect_files_to_txt(current_dir)
