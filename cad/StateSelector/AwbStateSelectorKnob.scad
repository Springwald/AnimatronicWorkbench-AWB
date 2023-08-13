/*


 ########################################
 # Animatronic Workbench State Selector #
 ########################################

 Licensed under MIT License (MIT)

 Copyright (c) 2023 Daniel Springwald | daniel@springwald.de

 Permission is hereby granted, free of charge, to any person obtaining
 a copy of this software and associated documentation files (the
 "Software"), to deal in the Software without restriction, including
 without limitation the rights to use, copy, modify, merge, publish,
 distribute, sublicense, and/or sell copies of the Software, and to permit
 persons to whom the Software is furnished to do so, subject to
 the following conditions:

 The above copyright notice and this permission notice shall be
 included in all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 DEALINGS IN THE SOFTWARE.

*/

$exportQuality = true;

function resolutionLow() = ($exportQuality==true) ? 20 : 10;
function resolutionHi() = ($exportQuality==true) ? 300 : 50;

include <LX-16A-Servo.scad>

module Knob() {
    
    difference() {
        union() {
            // Body
            cylinder(d=30, h=10,$fn=resolutionHi(), center=true);
            translate([1.5,0,4.5])
                cube([10,15,5], center=true);
            
            // direction triangle
            translate([3.6,0,-5])
                difference() {
                    cylinder(d=40,h=12,$fn=3);
                        translate([-15,0,00]){
                            cylinder(d=35,h=20,$fn=3);
                        }
            }
        }
        union()  {
            // servo screw holes
            translate([0,0,-25])
                color([1,0,1]) LX16AAxis(center=true);
            translate([0,0,-13.5])
                LX16AAxisScrewDriverTunnels(center=true);
        }
    }
    translate([0,0,4.5])
        color([1,0,0]) cylinder(d=8, h=5,$fn=resolutionHi(), center=true);
    
}

module ScrewHoles() 
{
    margin = 1;
    height = 30;
    
    depthFront = LX16AOuterBodyDepth + LX16AidHolderDepth + LX16AlogoHolderDepth + margin ;
    
    // top screw holes
    screwDriverHoleRadius = 4;
    translate([+LX16AHoleDistanceX/2,4.5, depthFront/2  + height/2]) cylinder(h=height, r=screwDriverHoleRadius, $fn=resolutionLow(), center=true); 
    translate([-LX16AHoleDistanceX/2,4.5, depthFront/2  + height/2]) cylinder(h=height, r=screwDriverHoleRadius, $fn=resolutionLow(), center=true);
    
    // bottom screw holes logo side
    screwHoleRadius = 0.8;
    translate([+LX16AHoleDistanceX/2,-16.3,depthFront/2  + height/2]) cylinder(h=height, r=screwDriverHoleRadius, $fn=resolutionLow(), center=true); 
    translate([-LX16AHoleDistanceX/2,-16.3,depthFront/2  + height/2]) cylinder(h=height, r=screwDriverHoleRadius, $fn=resolutionLow(), center=true);
    
    // bottom screw holes id side
    translate([+LX16AHoleDistanceX/2,-20.5,-depthFront/2 - height/2 -margin]) cylinder(h=height, r=screwDriverHoleRadius, $fn=resolutionLow(), center=true); 
    translate([-LX16AHoleDistanceX/2,-20.5,-depthFront/2 - height/2 -margin]) cylinder(h=height, r=screwDriverHoleRadius, $fn=resolutionLow(), center=true);
}

Knob();

// optional spacer;
if (false) {
    translate([50,10,0])
        scale([1,1,0.3])
            LX16ACircleHolder();
}