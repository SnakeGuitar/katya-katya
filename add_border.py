import sys
from PIL import Image, ImageFilter
import os

def add_white_border(image_path, border_size=15):
    try:
        # Open the image
        img = Image.open(image_path).convert("RGBA")
        
        # Split into RGB and Alpha
        r, g, b, a = img.split()
        
        # Dilate the alpha channel
        dilated_alpha = a.filter(ImageFilter.MaxFilter(size=border_size * 2 + 1))
        
        # Create a solid white background with the dilated alpha
        white_bg = Image.new("RGBA", img.size, (255, 255, 255, 255))
        white_bg.putalpha(dilated_alpha)
        
        # Paste the original image over the white background
        white_bg.alpha_composite(img)
        
        # Save the result, overwriting the original file
        white_bg.save(image_path)
        print(f"Added border to {image_path}")
    except Exception as e:
        print(f"Failed processing {image_path}: {e}")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python add_border.py <path_to_image1> [path_to_image2 ...]")
        print("Example: python add_border.py image.png another_image.png")
    else:
        for img_path in sys.argv[1:]:
            if os.path.exists(img_path):
                add_white_border(img_path, border_size=15)
            else:
                print(f"File not found: {img_path}")
