import os

directory = r"c:\Users\snake\Workspace\Memory-Game-Revival\MemoryGame-Revival\MemoryGame.Client\Views"

for root, _, files in os.walk(directory):
    for file in files:
        if file.endswith(".xaml"):
            filepath = os.path.join(root, file)
            with open(filepath, "r", encoding="utf-8") as f:
                content = f.read()

            new_content = content.replace(
                'pack://application:,,,/Resources/Images/Backgrounds/background-minimalistic.png',
                '{DynamicResource GlobalBackgroundSource}'
            ).replace(
                '/Resources/Images/Backgrounds/background-minimalistic.png',
                '{DynamicResource GlobalBackgroundSource}'
            ).replace(
                'pack://application:,,,/Resources/Images/Backgrounds/katya-main-background-only.png',
                '{DynamicResource GlobalMainBackgroundSource}'
            ).replace(
                '/Resources/Images/Backgrounds/katya-main-background-only.png',
                '{DynamicResource GlobalMainBackgroundSource}'
            )

            if new_content != content:
                with open(filepath, "w", encoding="utf-8") as f:
                    f.write(new_content)
                print(f"Updated {filepath}")
