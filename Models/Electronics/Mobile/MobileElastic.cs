﻿using System;
using System.Collections.Generic;

namespace ClerkBot.Models.Electronics.Mobile
{
    public class MobileElastic
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public ImageData Image { get; set; }
        public Status Status { get; set; }
        public Name Name { get; set; }
        public Network Network { get; set; }
        public Body Body { get; set; }
        public Platform Platform { get; set; }
        public Memory Memory { get; set; }
        public Display Display { get; set; }
        public Camera Camera { get; set; }
        public Sound Sound { get; set; }
        public Comms Comms { get; set; }
        public Battery Battery { get; set; }
        public FeaturesDb Features { get; set; }
        public Dictionary<string, double> Price { get; set; }
        public List<string> Colors { get; set; }
    }

    public enum Cores
    {
        singlecore = 1,
        dualcore = 2,
        quadcore = 4,
        hexacore = 6,
        octacore = 8
    }

    public class ImageData
    {
        public List<byte[]> Images { get; set; }
    }

    public class Status
    {
        public string Announced { get; set; }
        public string Launch { get; set; }
    }

    public class Name
    {
        public string Brand { get; set; }
        public string Main { get; set; }
    }

    public class Network
    {
        public List<string> Technology { get; set; }
        public List<string> Speed { get; set; }
        public Dictionary<string, List<string>> Bands { get; set; }
    }

    public class Body
    {
        public List<string> Build { get; set; }
        public string Sim { get; set; }
        public double Weight { get; set; }
        public KeyValuePair<int, string> WaterResistant { get; set; }
        public Dimensions Dimensions { get; set; }
    }

    public class Dimensions
    {
        public Dictionary<string, double> Cm { get; set; }
        public Dictionary<string, double> In { get; set; }
    }

    public class Platform
    {
        public Dictionary<string, int> Os { get; set; }
        public List<Chipset> Chipset { get; set; }
        public List<CPU> CPU { get; set; }
        public List<GPU> GPU { get; set; }
        public List<Performance> Tests { get; set; }
    }

    public class Performance
    {
        public string Type { get; set; }
        public double Score { get; set; }
    }

    public class Chipset
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Generation { get; set; }
        public double Size { get; set; }
    }

    public class CPU
    {
        public string Type { get; set; }
        public int Cores { get; set; }
        public List<CpuType> CpuList { get; set; }
    }

    public class CpuType
    {
        public int Count { get; set; }
        public double Ghz { get; set; }
        public string Name { get; set; }
    }

    public class GPU
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Generation { get; set; }
    }

    public class Memory
    {
        public string Type { get; set; }

        public List<InternalMemory> Internals { get; set; }
        public bool CardSlot { get; set; }
    }

    public class InternalMemory
    {
        public int Size { get; set; }
        public int RAM { get; set; }
    }

    public class Display
    {
        public List<string> RefreshRate { get; set; }
        public Size Size { get; set; }
        public Resolution Resolution { get; set; }
        public KeyValuePair<string, int> Protection { get; set; }
        public KeyValuePair<string, string> Type { get; set; }
    }

    public class Size
    {
        public double Cm { get; set; }
        public double In { get; set; }
        public KeyValuePair<string, double> BodyRatio { get; set; }
    }

    public class Resolution
    {
        public int Height { get; set; }
        public int Weight { get; set; }
        public int Density { get; set; }
        public string Ratio { get; set; }
    }

    public class Camera
    {
        public MainCamera Main { get; set; }
        public SelfieCamera Selfie { get; set; }
    }

    public class MainCamera
    {
        public int Number { get; set; }
        public List<LensCamera> Lens { get; set; }
        public List<string> Features { get; set; }
        public Dictionary<int, List<string>> Videos { get; set; }
    }

    public class SelfieCamera
    {
        public int Number { get; set; }
        public List<LensCamera> Lens { get; set; }
        public List<string> Features { get; set; }
        public Dictionary<int, List<string>> Videos { get; set; }
    }

    public class LensCamera
    {
        public string Type { get; set; }
        public double Size { get; set; }
        public double Megapixels { get; set; }
        public double Aperture { get; set; }
        public double Micro { get; set; }
        public Dictionary<double, string> Zoom { get; set; }
    }

    public class Sound
    {
        public bool Stereo { get; set; }
        public bool Loudspeaker { get; set; }
        public bool Jack { get; set; }
        public List<string> Others { get; set; }
    }

    public class Comms
    {
        public List<string> GPS { get; set; }
        public double Bluetooth { get; set; }
        public List<string> Wifi { get; set; }
        public KeyValuePair<string, double> Usb { get; set; }
    }

    public class FeaturesDb
    {
        public bool WaterResistant { get; set; }
        public bool DustResistant { get; set; }
        public bool Radio { get; set; }
        public bool FastCharging { get; set; }
        public bool Jack { get; set; }
        public bool Nfc { get; set; }
        public bool Gps { get; set; }
        public bool Gyro { get; set; }
        public KeyValuePair<bool, string> Fingerprint { get; set; }
    }

    public class Battery
    {
        public int Capacity { get; set; }
        public bool Fast { get; set; }
        public int Power { get; set; }
        public bool Wireless { get; set; }
    }
}
