using System;
using System.Text;

namespace Bev.Instruments.OceanOptics.Usb2000
{
    public partial class OceanOpticsUsb2000
    {
        private string GetSpectrometerType()
        {
            string result = string.Empty;
            try
            {
                byte[] slot = new byte[SeaBreezeWrapper.SLOT_LENGTH];
                int error = 0;
                SeaBreezeWrapper.seabreeze_get_model(specIndex, ref error, ref slot[0], SeaBreezeWrapper.SLOT_LENGTH);
                if (checkSeaBreezeError("get_spectrometer_type", error))
                    result = ByteToString(slot);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting spectrometer type: {e}");
            }
            finally
            {
            }
            return result;
        }

        private string GetSerialNumber()
        {
            string result = string.Empty;
            try
            {
                byte[] slot = new byte[SeaBreezeWrapper.SLOT_LENGTH];
                int error = 0;
                SeaBreezeWrapper.seabreeze_get_serial_number(specIndex, ref error, ref slot[0], SeaBreezeWrapper.SLOT_LENGTH);
                if (checkSeaBreezeError("get_serial_number", error))
                    result = ByteToString(slot);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting serial number: {e.Message}");
            }
            finally
            {
            }
            return result;
        }

        private string GetVersion()
        {
            string result = string.Empty;
            const int MAX_VERSION_LEN = 80;
            try
            {
                byte[] version = new byte[MAX_VERSION_LEN];
                int error = 0;
                SeaBreezeWrapper.seabreeze_get_api_version_string(ref version[0], version.Length);
                if (checkSeaBreezeError("get_api_version_string", error))
                    result = ByteToString(version);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting version string: {e.Message}");
            }
            finally
            {
            }
            return result;
        }

        private bool SetIntegrationTimeMilliseconds(double ms)
        {
            bool result = false;
            try
            {
                int error = 0;
                SeaBreezeWrapper.seabreeze_set_integration_time_microsec(specIndex, ref error, (long)(ms * 1000));
                result = checkSeaBreezeError("set_integration_time_microsec", error);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error setting integration time: {e}");
            }
            finally
            {
            }
            return result;
        }

        private double GetMinIntegrationTimeSec()
        {
            // long seabreeze_get_min_integration_time_microsec(int index, ref int errorCode);
            int error = 0;
            long intTimeMicrosec = SeaBreezeWrapper.seabreeze_get_min_integration_time_microsec(specIndex, ref error);
            bool result = checkSeaBreezeError("seabreeze_get_min_integration_time_microsec", error);
            return (double)intTimeMicrosec*1.0e-6;
        }

        private bool OpenAndInitialize()
        {
            wavelengthsCache = null;
            int error = 0;
            SeaBreezeWrapper.seabreeze_open_spectrometer(specIndex, ref error);
            if (!checkSeaBreezeError("open_spectrometer", error))
                return false;
            nPixels = SeaBreezeWrapper.seabreeze_get_formatted_spectrum_length(specIndex, ref error);
            if (!checkSeaBreezeError("get_formatted_spectrum_length", error))
                return false;
            double[] tmp = new double[nPixels];
            SeaBreezeWrapper.seabreeze_get_wavelengths(specIndex, ref error, ref tmp[0], nPixels);
            if (!checkSeaBreezeError("get_wavelengths", error))
                return false;
            wavelengthsCache = tmp;
            return true;
        }

        private void ReadSacrificialSpectrum() => GetIntensityData(); // after setting a new integration time

        // returns true if last operation was successful, false if last operation had an error
        private bool checkSeaBreezeError(string operation, int errorCode)
        {
            if (errorCode == SeaBreezeWrapper.ERROR_SUCCESS)
                return true;
            byte[] buffer = new byte[64];
            SeaBreezeWrapper.seabreeze_get_error_string(errorCode, ref buffer[0], 64);
            string msg = ByteToString(buffer);
            Console.WriteLine($"[SeaBreeze] error during {operation}: {msg}");
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
