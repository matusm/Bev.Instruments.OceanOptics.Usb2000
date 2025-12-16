using Bev.Instruments.ArraySpectrometer.Abstractions;
using System;

namespace Bev.Instruments.OceanOptics.Usb2000
{
    public partial class OceanOpticsUsb2000 : IArraySpectrometer
    {
        private int nPixels;
        private readonly int specIndex;
        private double[] wavelengthsCache;
        private double integrationTimeSeconds;

        public OceanOpticsUsb2000() : this(0) { } // for now, hardcode to use first enumerated spectrometer

        public OceanOpticsUsb2000(int specIndex)
        {
            this.specIndex = specIndex;
            if(!OpenAndInitialize())
                throw new Exception("Failed to open and initialize Ocean Optics USB2000 spectrometer.");
            SetIntegrationTime(MinimumIntegrationTime);
        }

        public string InstrumentManufacturer => "Ocean Optics";
        public string InstrumentType => GetSpectrometerType();
        public string InstrumentSerialNumber => GetSerialNumber();
        public string InstrumentFirmwareVersion => GetVersion();
        public double[] Wavelengths => wavelengthsCache;
        public double MinimumWavelength => wavelengthsCache[0];
        public double MaximumWavelength => wavelengthsCache[wavelengthsCache.Length - 1];
        public double SaturationLevel => 4000;
        public double MinimumIntegrationTime => GetMinIntegrationTimeSec();
        public double MaximumIntegrationTime => 65.0;

        public double GetIntegrationTime() => integrationTimeSeconds; // there is no SeaBreeze call to read it back

        public double[] GetIntensityData()
        {
            double[] result = null;
            try
            {
                int error = 0;
                double[] spec = new double[nPixels];
                SeaBreezeWrapper.seabreeze_get_formatted_spectrum(specIndex, ref error, ref spec[0], nPixels);
                if (checkSeaBreezeError("get_formatted_spectrum", error))
                {
                    // KLUDGE: Some spectrometers (e.g. HR4000) insert non-pixel data 
                    // into the first few pixels of the spectrum they report, which
                    // we can override here to avoid confusing EDC and stray noise on
                    // graphs.
                    // 
                    // TODO: Put in appropriate logic based on spectrometer model.
                    for (int i = 0; i < 5; i++)
                        spec[i] = spec[5];
                    result = spec;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting spectrum: {e}");
            }
            finally
            {
            }
            return result;
        }

        public void SetIntegrationTime(double seconds)
        {
            if (seconds < MinimumIntegrationTime) seconds = MinimumIntegrationTime;
            if (seconds > MaximumIntegrationTime) seconds = MaximumIntegrationTime;
            integrationTimeSeconds = seconds;
            SetIntegrationTimeMilliseconds(seconds * 1000.0);
            ReadSacrificialSpectrum(); // this seems to be necessarry for this type of spectrometers
        }

    }
}
