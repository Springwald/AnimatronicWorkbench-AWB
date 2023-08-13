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

module StateSelectorFront() {
    
    // Panel holder
    difference() {
        cube([110,90,5], center=true);
        translate([0,34,0])
            cube([25,125,10], center=true);
    }
    
    // Servo holder
    translate([0,-5,-15.875])
        LX16ACubeHolder(); 
    
   
    // Foot
    translate([0,-27.25,-43.9])
        rotate([-70,0,0]){
            cube([50,90,5], center=true);
        }
}


StateSelectorFront();

