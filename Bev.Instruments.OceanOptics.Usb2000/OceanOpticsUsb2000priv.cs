using System;
using System.Text;

namespace Bev.Instruments.OceanOptics.Usb2000
{
    public partial class OceanOpticsUsb2000
    {
        private string GetSpectrometerType()
        {
            byte[] slot = new byte[SeaBreezeWrapper.SLOT_LENGTH];
            int error = 0;
            SeaBreezeWrapper.seabreeze_get_model(_specIndex, ref error, ref slot[0], SeaBreezeWrapper.SLOT_LENGTH);
            if (IfSeaBreezeSuccess("get_spectrometer_type", error))
                return ByteToString(slot);
            return string.Empty;
        }

        private string GetSerialNumber()
        {
            byte[] slot = new byte[SeaBreezeWrapper.SLOT_LENGTH];
            int error = 0;
            SeaBreezeWrapper.seabreeze_get_serial_number(_specIndex, ref error, ref slot[0], SeaBreezeWrapper.SLOT_LENGTH);
            if (IfSeaBreezeSuccess("get_serial_number", error))
                return ByteToString(slot);
            return string.Empty;
        }

        private string GetVersion()
        {
            byte[] version = new byte[SeaBreezeWrapper.MAX_VERSION_LEN];
            int error = 0;
            SeaBreezeWrapper.seabreeze_get_api_version_string(ref version[0], version.Length);
            if (IfSeaBreezeSuccess("get_api_version_string", error))
                return ByteToString(version);
            return string.Empty;
        }

        private void SetIntegrationTimeMilliseconds(double ms)
        {
            int error = 0;
            SeaBreezeWrapper.seabreeze_set_integration_time_microsec(_specIndex, ref error, (long)(ms * 1000));
            _ = IfSeaBreezeSuccess("set_integration_time_microsec", error);
        }

        private double GetMinIntegrationTimeSec()
        {
            int error = 0;
            long intTimeMicrosec = SeaBreezeWrapper.seabreeze_get_min_integration_time_microsec(_specIndex, ref error);
            _ = IfSeaBreezeSuccess("seabreeze_get_min_integration_time_microsec", error);
            return (double)intTimeMicrosec * 1.0e-6;
        }

        private bool OpenAndInitialize()
        {
            _wavelengthsCache = null;
            int error = 0;
            SeaBreezeWrapper.seabreeze_open_spectrometer(_specIndex, ref error);
            if (!IfSeaBreezeSuccess("open_spectrometer", error))
                return false;
            _nPixels = SeaBreezeWrapper.seabreeze_get_formatted_spectrum_length(_specIndex, ref error);
            if (!IfSeaBreezeSuccess("get_formatted_spectrum_length", error))
                return false;
            double[] tmp = new double[_nPixels];
            SeaBreezeWrapper.seabreeze_get_wavelengths(_specIndex, ref error, ref tmp[0], _nPixels);
            if (!IfSeaBreezeSuccess("get_wavelengths", error))
                return false;
            _wavelengthsCache = tmp;
            return true;
        }

        // returns true if last operation was successful, false if last operation had an error
        private bool IfSeaBreezeSuccess(string operation, int errorCode, bool debug = false)
        {
            if (errorCode == SeaBreezeWrapper.ERROR_SUCCESS)
                return true;
            if (debug)
            {
                byte[] buffer = new byte[64];
                SeaBreezeWrapper.seabreeze_get_error_string(errorCode, ref buffer[0], 64);
                string msg = ByteToString(buffer);
                Console.WriteLine($"[SeaBreeze] Debug: operation {operation} returned error code {errorCode}: {msg}");
            }
            return false;
        }

        private string ByteToString(byte[] buf)
        {
            int len = 0;
            while (buf[len] != 0 && len + 1 < buf.Length)
                len++;
            byte[] clean = Encoding.Convert(Encoding.GetEncoding("iso-8859-1"), Encoding.UTF8, buf);
            return Encoding.UTF8.GetString(clean, 0, len);
        }

    }
}
