#!/bin/sh

src=${1:-AppLauncher.svg}
dest=../JumpListAppLauncher/Assets/AppIcons

# drawWithMargin src.svg 100x100 dst.png 310x310
function drawMargin(){
    magick -background none -units PixelsPerInch $1 -resize $2 -gravity center -background none -extent $4 -density 144 -strip $3
}

# draw src.svg 100x100 dst.png
function draw(){
    magick -background none -units PixelsPerInch $1 -resize $2 -density 144 -strip $3
}

# https://learn.microsoft.com/en-us/windows/apps/design/style/iconography/app-icon-construction

rm $dest/*

magick -background none $src -resize 16x16 $dest/../AppLauncher.ico


# like a WinUI-Gallery
drawMargin $src     14x14 $dest/AppList.targetsize-16.png 16x16
drawMargin $src     18x18 $dest/AppList.targetsize-20.png 20x20
drawMargin $src     22x22 $dest/AppList.targetsize-24.png 24x24
drawMargin $src     26x26 $dest/AppList.targetsize-30.png 30x30
drawMargin $src     28x28 $dest/AppList.targetsize-32.png 32x32
drawMargin $src     32x32 $dest/AppList.targetsize-36.png 36x36
drawMargin $src     36x36 $dest/AppList.targetsize-40.png 40x40
drawMargin $src     44x44 $dest/AppList.targetsize-48.png 48x48
drawMargin $src     52x52 $dest/AppList.targetsize-60.png 60x60
drawMargin $src     58x58 $dest/AppList.targetsize-64.png 64x64
drawMargin $src     64x64 $dest/AppList.targetsize-72.png 72x72
drawMargin $src     72x72 $dest/AppList.targetsize-80.png 80x80
drawMargin $src     88x88 $dest/AppList.targetsize-96.png 96x96
drawMargin $src     232x232 $dest/AppList.targetsize-256.png 256x256
for file in $dest/AppList.targetsize-*.png; do
  cp "$file" "${file%.png}_altform-lightunplated.png"
  cp "$file" "${file%.png}_altform-unplated.png"
done


drawMargin $src     28x28 $dest/AppList.scale-100.png 44x44
drawMargin $src     36x36 $dest/AppList.scale-125.png 55x55
drawMargin $src     44x44 $dest/AppList.scale-150.png 66x66
drawMargin $src     58x58 $dest/AppList.scale-200.png 88x88
drawMargin $src     116x116 $dest/AppList.scale-400.png 176x176
for file in $dest/AppList.scale-*.png; do
  cp "$file" "${file%.png}_altform-colorful_theme-light.png"
done

drawMargin $src     32x32 $dest/SmallTile.scale-100.png 71x71
drawMargin $src     44x44 $dest/SmallTile.scale-125.png 89x89
drawMargin $src     50x50 $dest/SmallTile.scale-150.png 107x107
drawMargin $src     64x64 $dest/SmallTile.scale-200.png 142x142
drawMargin $src     128x128 $dest/SmallTile.scale-400.png 284x284

drawMargin $src     44x44 $dest/MedTile.scale-100.png 150x150
drawMargin $src     58x58 $dest/MedTile.scale-125.png 188x188
drawMargin $src     72x72 $dest/MedTile.scale-150.png 225x225
drawMargin $src     88x88 $dest/MedTile.scale-200.png 300x300
drawMargin $src     176x176 $dest/MedTile.scale-400.png 600x600

drawMargin $src     44x44 $dest/WideTile.scale-100.png 310x150
drawMargin $src     58x58 $dest/WideTile.scale-125.png 388x188
drawMargin $src     72x72 $dest/WideTile.scale-150.png 465x225
drawMargin $src     88x88 $dest/WideTile.scale-200.png 620x300
drawMargin $src     176x176 $dest/WideTile.scale-400.png 1240x600

drawMargin $src     88x88 $dest/LargeTile.scale-100.png 310x310
drawMargin $src     116x116 $dest/LargeTile.scale-125.png 388x388
drawMargin $src     132x132 $dest/LargeTile.scale-150.png 465x465
drawMargin $src     176x176 $dest/LargeTile.scale-200.png 620x620
drawMargin $src     352x352 $dest/LargeTile.scale-400.png 1240x1240
for file in $dest/SmallTile.scale-*.png $dest/MedTile.scale-*.png $dest/WideTile.scale-*.png $dest/LargeTile.scale-*.png; do
  cp "$file" "${file%.png}_altform-colorful_theme-light.png"
done


drawMargin $src     88x88 $dest/SplashScreen.scale-100.png 620x300
drawMargin $src     116x116 $dest/SplashScreen.scale-125.png 775x375
drawMargin $src     132x132 $dest/SplashScreen.scale-150.png 930x450
drawMargin $src     176x176 $dest/SplashScreen.scale-200.png 1240x600
drawMargin $src     352x352 $dest/SplashScreen.scale-400.png 2480x1200
for file in $dest/SplashScreen.scale-*.png; do
  cp "$file" "${file%.png}_altform-colorful_theme-dark.png"
  cp "$file" "${file%.png}_altform-colorful_theme-light.png"
done

#drawMargin $src      22x22 $dest/BadgeLogo.scale-100.png 24x24
#drawMargin $src      26x26 $dest/BadgeLogo.scale-125.png 30x30
#drawMargin $src      32x32 $dest/BadgeLogo.scale-150.png 36x36
#drawMargin $src      44x44 $dest/BadgeLogo.scale-200.png 48x48
#drawMargin $src      88x88 $dest/BadgeLogo.scale-400.png 96x96


#drawMargin $src      352x352 $dest/StoreDisplay-71.png 2480x1200

drawMargin $src      32x32 $dest/StoreLogo.scale-100.png 50x50
drawMargin $src      44x44 $dest/StoreLogo.scale-125.png 63x63
drawMargin $src      50x50 $dest/StoreLogo.scale-150.png 75x75
drawMargin $src      64x64 $dest/StoreLogo.scale-200.png 100x100
drawMargin $src      128x128 $dest/StoreLogo.scale-400.png 200x200
for file in $dest/StoreLogo.scale-*.png; do
  cp "$file" "${file%.png}_altform-colorful_theme-light.png"
done
