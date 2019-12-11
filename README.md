# 8bitVR
Development Repo for Virtual Reality with 8bi device interaction

# Project Used the following tools:
- Engine:   Unity v2019.1.6f1 https://unity.com/
- Language: C#, Visual Studio Code  https://code.visualstudio.com/
- VR Asset (Teleport/Hands): Steam VR Plugin v2.5.0 (sdk 1.8.19) https://assetstore.unity.com/publishers/12026
- Unity Asset (Serial Communication): Ardity v1.1.0 http://ardity.dwilches.com/ 

# Hardware Requirements:
Any device cable of Serial Communication.
Use Case:   MCP2221 USB<-->Serial Bridge Device w/8bit embedded device (PIC|AVR)
Test VR Hardware:   HTC Vive, Oculus Rift-S
PIC16LF18456, ATMega4809, PIC18F47K40

# Communication Formats
- Testing (Legacy Format)
    - Message is split on the ' ' character
    - 0   1  2 3 4 5 6 7 8 9 10 11 12 13 14 
    - T +/-# . # A X # Y # Z #  P 0/1 L  0/1
    - E.G. T +25 . 625 A X 10 Y -50 Z 192 P 0 L 1

    0 - Temperature Data
    1 - Positive/Negative Sym  w/ 0-128 celsius whole value
    2 - . character
    3 - .0-9999 decimal range
    4 - Accelerometer Data 
    5 - X Value
    6 - 12-bit value, msb sign
    7 - Y Value
    8 - 12-bit value, msb sign
    9 - Z Value
    10 - 12-bit value, msb sign
    11 - Push Button Data
    12 - 0 / 1 = Pressed / Released
    13 - LED Data
    14 - 0 / 1 = ON / OFF

- JSON (like)
    - { withouth a ? following starts Parsing of message.
    - Similar to {JSON:FOMAT, MESSAGE:CODING}
    - { Key:Value, Key:Value }
    - E.G. {T:+25.625,X:10,Y:-50,Z:192,P:0,L:1,G:3}

    - Keys
        - 'T' : Temperature
        - 'L' : LEDs
        - 'P' : Push Button
        - 'X' : X -> Accelerometer
        - 'Y' : Y -> Accelerometer
        - 'Z' : Z -> Accelerometer
        - 'G' : GPIOs

    - Value Types
        - String (All)
