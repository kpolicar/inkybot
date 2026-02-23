from PIL import Image
import os
os.chdir(r'A:\Projects\RiderProjects\inkybot\Tests\Resources\Screenshots\crops')

img = Image.open('crop_3.png')
pixels = img.load()
w, h = img.size

for row_y in [15, 55, 98, 141, 184, 227, 270, 313, 356, 399, 442]:
    line = ''
    for x in range(w):
        r,g,b = pixels[x,row_y][:3]
        brightness = (r+g+b)//3
        if brightness > 150:
            line += '#'
        elif brightness > 80:
            line += '.'
        else:
            line += ' '
    print("y={:3d}: |{}|".format(row_y, line))

print()
markers = ''
for x in range(w):
    if x % 50 == 0:
        markers += str(x).ljust(1)
    elif x % 10 == 0:
        markers += '|'
    else:
        markers += ' '
print("x:      |{}|".format(markers))

