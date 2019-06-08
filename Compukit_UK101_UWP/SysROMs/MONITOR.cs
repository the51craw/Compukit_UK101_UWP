using System;

namespace Compukit_UK101_UWP
{
    public class MONITOR : CMemoryBusDevice
    {
        public UInt16 ROMSize { get; set; }

        public MONITOR(Address Address)
        {
            this.Address = Address;
	        ReadOnly = true;
        }

        public byte[] pData = new byte[] { 
            0xa5, 0x0e, 0xf0, 0x06, 0xc6, 0x0e, 0xf0, 0x02, 0xc6, 0x0e, 0xa9, 0x20, 0x8d, 0x01, 0x02, 0x20, // F800 - F80F
			0x8f, 0xff, 0x10, 0x19, 0x38, 0xad, 0x2b, 0x02, 0xe9, 0x40, 0x8d, 0x2b, 0x02, 0xad, 0x2c, 0x02, // F810
			0xe9, 0x00, 0x8d, 0x2c, 0x02, 0x20, 0xcf, 0xfb, 0xb0, 0x03, 0x20, 0xd1, 0xff, 0x8e, 0x00, 0x02, // F820
			0x20, 0x88, 0xff, 0x4c, 0xd2, 0xf8, 0x8d, 0x02, 0x02, 0x48, 0x8a, 0x48, 0x98, 0x48, 0xad, 0x02, // F830
			0x02, 0xd0, 0x03, 0x4c, 0xd2, 0xf8, 0xac, 0x06, 0x02, 0xf0, 0x03, 0x20, 0xe1, 0xfc, 0xc9, 0x5f, // F840
			0xf0, 0xae, 0xc9, 0x0c, 0xd0, 0x0b, 0x20, 0x8c, 0xff, 0x20, 0xd1, 0xff, 0x8e, 0x00, 0x02, 0xf0, // F850
			0x6e, 0xc9, 0x0a, 0xf0, 0x27, 0xc9, 0x1e, 0xf0, 0x77, 0xc9, 0x0b, 0xf0, 0x10, 0xc9, 0x1a, 0xf0, // F860
			0x67, 0xc9, 0x0d, 0xd0, 0x05, 0x20, 0x6d, 0xff, 0xd0, 0x58, 0x8d, 0x01, 0x02, 0x20, 0x8c, 0xff, // F870
			0xee, 0x00, 0x02, 0xe8, 0xec, 0x22, 0x02, 0x30, 0x46, 0x20, 0x70, 0xff, 0x20, 0x8c, 0xff, 0xa0, // F880
			0x02, 0x20, 0xd2, 0xfb, 0xb0, 0x08, 0xa2, 0x03, 0x20, 0xee, 0xfd, 0x4c, 0xcf, 0xf8, 0x20, 0x28, // F890
			0xfe, 0x20, 0xd1, 0xff, 0x20, 0xee, 0xfd, 0xae, 0x22, 0x02, 0x20, 0x27, 0x02, 0x10, 0xfb, 0xe8, // F8A0
			0x20, 0xee, 0xfd, 0xa2, 0x03, 0x20, 0xee, 0xfd, 0x20, 0xcf, 0xfb, 0x90, 0xed, 0xa9, 0x20, 0x20, // F8B0
			0x2a, 0x02, 0x10, 0xfb, 0xa2, 0x01, 0xbd, 0x23, 0x02, 0x9d, 0x28, 0x02, 0xca, 0x10, 0xf7, 0x20, // F8C0
			0x75, 0xff, 0x68, 0xa8, 0x68, 0xaa, 0x68, 0x60, 0x20, 0x59, 0xfe, 0x8d, 0x01, 0x02, 0xf0, 0x24, // F8D0
			0xa9, 0x20, 0x20, 0x8f, 0xff, 0x20, 0xd1, 0xff, 0xae, 0x22, 0x02, 0xa9, 0x20, 0x20, 0x2a, 0x02, // F8E0
			0x10, 0xfb, 0x8d, 0x01, 0x02, 0xa0, 0x02, 0x20, 0xd2, 0xfb, 0xb0, 0x08, 0xa2, 0x03, 0x20, 0xee, // F8F0
			0xfd, 0x4c, 0xe8, 0xf8, 0x20, 0xd1, 0xff, 0x8e, 0x00, 0x02, 0xf0, 0xc6, 0x20, 0xa6, 0xf9, 0x20, // F900
			0xf5, 0xfb, 0x20, 0xb6, 0xfe, 0x20, 0xe6, 0xfb, 0x20, 0xe0, 0xfb, 0xa2, 0x08, 0x86, 0xfd, 0x20, // F910
			0xe6, 0xfb, 0x20, 0xf0, 0xfe, 0x20, 0xeb, 0xfb, 0xb0, 0x51, 0x20, 0xf9, 0xfe, 0xc6, 0xfd, 0xd0, // F920
			0xee, 0xf0, 0xdc, 0x20, 0xbd, 0xff, 0x20, 0xe4, 0xfd, 0xb0, 0x43, 0xa6, 0xe4, 0x9a, 0xa5, 0xe6, // F930
			0x48, 0xa5, 0xe5, 0x48, 0xa5, 0xe3, 0x48, 0xa5, 0xe0, 0xa6, 0xe1, 0xa4, 0xe2, 0x40, 0xa2, 0x03, // F940
			0xbd, 0x4b, 0xfa, 0x9d, 0xbf, 0x01, 0xca, 0xd0, 0xf7, 0x20, 0x8d, 0xfe, 0x20, 0xb5, 0xf9, 0xb1, // F950
			0xfe, 0x85, 0xe7, 0x98, 0x91, 0xfe, 0xf0, 0x16, 0x4c, 0x7e, 0xfa, 0xc6, 0xfb, 0xd0, 0x79, 0xf0, // F960
			0x9b, 0x60, 0xa5, 0xfb, 0xd0, 0xfb, 0xa9, 0x3f, 0x20, 0xee, 0xff, 0xa2, 0x28, 0x9a, 0x20, 0xf5, // F970
			0xfb, 0xa0, 0x00, 0x84, 0xfb, 0x20, 0xe0, 0xfb, 0x20, 0x8d, 0xfe, 0xc9, 0x4d, 0xf0, 0xa4, 0xc9, // F980
			0x52, 0xf0, 0xa8, 0xc9, 0x5a, 0xf0, 0xb7, 0xc9, 0x53, 0xf0, 0xcd, 0xc9, 0x4c, 0xf0, 0xcc, 0xc9, // F990
			0x55, 0xd0, 0x33, 0x6c, 0x33, 0x02, 0x20, 0x8d, 0xfe, 0x20, 0xb5, 0xf9, 0x20, 0xe3, 0xfb, 0xa2, // F9A0
			0x00, 0x20, 0x8d, 0xfe, 0x2c, 0xa2, 0x05, 0x20, 0xc0, 0xf9, 0x20, 0x8d, 0xfe, 0x2c, 0xa2, 0x03, // F9B0
			0x20, 0xc6, 0xf9, 0x20, 0x8d, 0xfe, 0xc9, 0x2e, 0xf0, 0xbe, 0xc9, 0x2f, 0xf0, 0x1a, 0x20, 0x93, // F9C0
			0xfe, 0x30, 0x9f, 0x4c, 0xda, 0xfe, 0xc9, 0x54, 0xf0, 0x95, 0x20, 0xb5, 0xf9, 0xa9, 0x2f, 0x20, // F9D0
			0xee, 0xff, 0x20, 0xf0, 0xfe, 0x20, 0xe6, 0xfb, 0x20, 0x8d, 0xfe, 0xc9, 0x47, 0xd0, 0x03, 0x6c, // F9E0
			0xfe, 0x00, 0xc9, 0x2c, 0xd0, 0x06, 0x20, 0xf9, 0xfe, 0x4c, 0xe8, 0xf9, 0xc9, 0x0a, 0xf0, 0x16, // F9F0
			0xc9, 0x0d, 0xf0, 0x17, 0xc9, 0x5e, 0xf0, 0x19, 0xc9, 0x27, 0xf0, 0x2e, 0x20, 0xbe, 0xf9, 0xa5, // FA00
			0xfc, 0x91, 0xfe, 0x4c, 0xe8, 0xf9, 0xa9, 0x0d, 0x20, 0xee, 0xff, 0x20, 0xf9, 0xfe, 0x4c, 0x31, // FA10
			0xfa, 0x38, 0xa5, 0xfe, 0xe9, 0x01, 0x85, 0xfe, 0xa5, 0xff, 0xe9, 0x00, 0x85, 0xff, 0x20, 0xf5, // FA20
			0xfb, 0x20, 0xb6, 0xfe, 0x4c, 0xdd, 0xf9, 0x20, 0xf7, 0xfe, 0x20, 0x8d, 0xfe, 0xc9, 0x27, 0xd0, // FA30
			0x05, 0x20, 0xe3, 0xfb, 0xd0, 0xcd, 0xc9, 0x0d, 0xf0, 0xe4, 0xd0, 0xeb, 0x4c, 0x4f, 0xfa, 0x85, // FA40
			0xe0, 0x68, 0x48, 0x29, 0x10, 0xd0, 0x03, 0xa5, 0xe0, 0x40, 0x86, 0xe1, 0x84, 0xe2, 0x68, 0x85, // FA50
			0xe3, 0xd8, 0x38, 0x68, 0xe9, 0x02, 0x85, 0xe5, 0x68, 0xe9, 0x00, 0x85, 0xe6, 0xba, 0x86, 0xe4, // FA60
			0xa0, 0x00, 0xa5, 0xe7, 0x91, 0xe5, 0xa9, 0xe0, 0x85, 0xfe, 0x84, 0xff, 0xd0, 0xb0, 0x20, 0xbd, // FA70
			0xff, 0x20, 0xf7, 0xff, 0x20, 0xe9, 0xfe, 0x20, 0xee, 0xff, 0x20, 0xe3, 0xff, 0xa9, 0x2f, 0x20, // FA80
			0xee, 0xff, 0xd0, 0x03, 0x20, 0xf9, 0xfe, 0x20, 0xf0, 0xfe, 0xa9, 0x0d, 0x20, 0xb1, 0xfc, 0x20, // FA90
			0xeb, 0xfb, 0x90, 0xf0, 0xa5, 0xe4, 0xa6, 0xe5, 0x85, 0xfe, 0x86, 0xff, 0x20, 0xe3, 0xff, 0xa9, // FAA0
			0x47, 0x20, 0xee, 0xff, 0x20, 0xac, 0xff, 0x8c, 0x05, 0x02, 0x4c, 0x7e, 0xf9, 0x8a, 0x48, 0x98, // FAB0
			0x48, 0xad, 0x04, 0x02, 0x10, 0x59, 0xac, 0x2f, 0x02, 0xad, 0x31, 0x02, 0x85, 0xe4, 0xad, 0x32, // FAC0
			0x02, 0x85, 0xe5, 0xb1, 0xe4, 0x8d, 0x30, 0x02, 0xa9, 0xa1, 0x91, 0xe4, 0x20, 0x00, 0xfd, 0xad, // FAD0
			0x30, 0x02, 0x91, 0xe4, 0xad, 0x15, 0x02, 0xc9, 0x11, 0xf0, 0x28, 0xc9, 0x01, 0xf0, 0x1e, 0xc9, // FAE0
			0x04, 0xf0, 0x14, 0xc9, 0x13, 0xf0, 0x0a, 0xc9, 0x06, 0xd0, 0x27, 0x20, 0x7c, 0xfb, 0x4c, 0xc6, // FAF0
			0xfa, 0x20, 0x28, 0xfe, 0x4c, 0xc6, 0xfa, 0x20, 0x6b, 0xfb, 0x4c, 0xc6, 0xfa, 0x20, 0x19, 0xfe, // FB00
			0x4c, 0xc6, 0xfa, 0xad, 0x30, 0x02, 0x8d, 0x15, 0x02, 0x20, 0x6b, 0xfb, 0x4c, 0x43, 0xfb, 0x20, // FB10
			0x00, 0xfd, 0xc9, 0x05, 0xd0, 0x1d, 0xad, 0x04, 0x02, 0x49, 0xff, 0x8d, 0x04, 0x02, 0x10, 0xef, // FB20
			0xad, 0x2b, 0x02, 0x8d, 0x31, 0x02, 0xad, 0x2c, 0x02, 0x8d, 0x32, 0x02, 0xa2, 0x00, 0x8e, 0x2f, // FB30
			0x02, 0xf0, 0x83, 0x4c, 0xd3, 0xfd, 0x2c, 0x03, 0x02, 0x10, 0x1d, 0xa9, 0xfd, 0x8d, 0x00, 0xdf, // FB40
			0xa9, 0x10, 0x2c, 0x00, 0xdf, 0xf0, 0x0a, 0xad, 0x00, 0xf0, 0x4a, 0x90, 0xee, 0xad, 0x01, 0xf0, // FB50
			0x60, 0xa9, 0x00, 0x85, 0xfb, 0x8d, 0x03, 0x02, 0x4c, 0xbd, 0xfa, 0xae, 0x22, 0x02, 0xec, 0x2f, // FB60
			0x02, 0xf0, 0x04, 0xee, 0x2f, 0x02, 0x60, 0xa2, 0x00, 0x8e, 0x2f, 0x02, 0x18, 0xad, 0x31, 0x02, // FB70
			0x69, 0x40, 0x8d, 0x31, 0x02, 0xad, 0x32, 0x02, 0x69, 0x00, 0xc9, 0xd8, 0xd0, 0x02, 0xa9, 0xd0, // FB80
			0x8d, 0x32, 0x02, 0x60, 0xad, 0x12, 0x02, 0xd0, 0xfa, 0xa9, 0xfe, 0x8d, 0x00, 0xdf, 0x2c, 0x00, // FB90
			0xdf, 0x70, 0xf0, 0xa9, 0xfb, 0x8d, 0x00, 0xdf, 0x2c, 0x00, 0xdf, 0x70, 0xe6, 0xa9, 0x03, 0x4c, // FBA0
			0x36, 0xa6, 0x46, 0xfb, 0x9b, 0xff, 0x94, 0xfb, 0x70, 0xfe, 0x7b, 0xfe, 0x2f, 0x4c, 0xd0, 0x4c, // FBB0
			0xd7, 0xbd, 0x8c, 0xd0, 0x9d, 0x8c, 0xd0, 0xca, 0x60, 0x00, 0x20, 0x8c, 0xd0, 0x88, 0xf9, 0xae, // FBC0
			0x22, 0x02, 0x38, 0xad, 0x2b, 0x02, 0xf9, 0x23, 0x02, 0xad, 0x2c, 0x02, 0xf9, 0x24, 0x02, 0x60, // FBD0
			0xa9, 0x3e, 0x2c, 0xa9, 0x2c, 0x2c, 0xa9, 0x20, 0x4c, 0xee, 0xff, 0x38, 0xa5, 0xfe, 0xe5, 0xf9, // FBE0
			0xa5, 0xff, 0xe5, 0xfa, 0x60, 0xa9, 0x0d, 0x20, 0xee, 0xff, 0xa9, 0x0a, 0x4c, 0xee, 0xff, 0x40, // FBF0
			0x20, 0x0c, 0xfc, 0x6c, 0xfd, 0x00, 0x20, 0x0c, 0xfc, 0x4c, 0x00, 0xfe, 0xa0, 0x00, 0x8c, 0x01, // FC00
			0xc0, 0x8c, 0x00, 0xc0, 0xa2, 0x04, 0x8e, 0x01, 0xc0, 0x8c, 0x03, 0xc0, 0x88, 0x8c, 0x02, 0xc0, // FC10
			0x8e, 0x03, 0xc0, 0x8c, 0x02, 0xc0, 0xa9, 0xfb, 0xd0, 0x09, 0xa9, 0x02, 0x2c, 0x00, 0xc0, 0xf0, // FC20
			0x1c, 0xa9, 0xff, 0x8d, 0x02, 0xc0, 0x20, 0xa5, 0xfc, 0x29, 0xf7, 0x8d, 0x02, 0xc0, 0x20, 0xa5, // FC30
			0xfc, 0x09, 0x08, 0x8d, 0x02, 0xc0, 0xa2, 0x18, 0x20, 0x91, 0xfc, 0xf0, 0xdd, 0xa2, 0x7f, 0x8e, // FC40
			0x02, 0xc0, 0x20, 0x91, 0xfc, 0xad, 0x00, 0xc0, 0x30, 0xfb, 0xad, 0x00, 0xc0, 0x10, 0xfb, 0xa9, // FC50
			0x03, 0x8d, 0x10, 0xc0, 0xa9, 0x58, 0x8d, 0x10, 0xc0, 0x20, 0x9c, 0xfc, 0x85, 0xfe, 0xaa, 0x20, // FC60
			0x9c, 0xfc, 0x85, 0xfd, 0x20, 0x9c, 0xfc, 0x85, 0xff, 0xa0, 0x00, 0x20, 0x9c, 0xfc, 0x91, 0xfd, // FC70
			0xc8, 0xd0, 0xf8, 0xe6, 0xfe, 0xc6, 0xff, 0xd0, 0xf2, 0x86, 0xfe, 0xa9, 0xff, 0x8d, 0x02, 0xc0, // FC80
			0x60, 0xa0, 0xf8, 0x88, 0xd0, 0xfd, 0x55, 0xff, 0xca, 0xd0, 0xf6, 0x60, 0xad, 0x10, 0xc0, 0x4a, // FC90
			0x90, 0xfa, 0xad, 0x11, 0xc0, 0x60, 0xa9, 0x03, 0x8d, 0x00, 0xf0, 0xa9, 0x11, 0x8d, 0x00, 0xf0, // FCA0
			0x60, 0x48, 0xad, 0x00, 0xf0, 0x4a, 0x4a, 0x90, 0xf9, 0x68, 0x8d, 0x01, 0xf0, 0x60, 0x49, 0xff, // FCB0
			0x8d, 0x00, 0xdf, 0x49, 0xff, 0x60, 0x48, 0x20, 0xcf, 0xfc, 0xaa, 0x68, 0xca, 0xe8, 0x60, 0xad, // FCC0
			0x00, 0xdf, 0x49, 0xff, 0x60, 0xc9, 0x5f, 0xf0, 0x03, 0x4c, 0x74, 0xa3, 0x4c, 0x4b, 0xa3, 0xa0, // FCD0
			0x10, 0xa2, 0x40, 0xca, 0xd0, 0xfd, 0x88, 0xd0, 0xf8, 0x60, 0x43, 0x45, 0x47, 0x4d, 0x4f, 0x4e, // FCE0
			0x28, 0x43, 0x29, 0x31, 0x39, 0x38, 0x30, 0x20, 0x44, 0x2f, 0x43, 0x2f, 0x57, 0x2f, 0x4d, 0x3f, // FCF0
			0x8a, 0x48, 0x98, 0x48, 0xa9, 0x80, 0x20, 0xbe, 0xfc, 0x20, 0xc6, 0xfc, 0xd0, 0x05, 0x4a, 0xd0, // FD00
			0xf5, 0xf0, 0x27, 0x4a, 0x90, 0x09, 0x8a, 0x29, 0x20, 0xf0, 0x1f, 0xa9, 0x1b, 0xd0, 0x31, 0x20, // FD10
			0x86, 0xfe, 0x98, 0x8d, 0x15, 0x02, 0x0a, 0x0a, 0x0a, 0x38, 0xed, 0x15, 0x02, 0x8d, 0x15, 0x02, // FD20
			0x8a, 0x4a, 0x0a, 0x20, 0x86, 0xfe, 0xf0, 0x0f, 0xa9, 0x00, 0x8d, 0x16, 0x02, 0x8d, 0x13, 0x02, // FD30
			0xa9, 0x02, 0x8d, 0x14, 0x02, 0xd0, 0xbd, 0x18, 0x98, 0x6d, 0x15, 0x02, 0xa8, 0xb9, 0x3b, 0xff, // FD40
			0xcd, 0x13, 0x02, 0xd0, 0xe8, 0xce, 0x14, 0x02, 0xf0, 0x05, 0x20, 0xdf, 0xfc, 0xf0, 0xa5, 0xa2, // FD50
			0x64, 0xcd, 0x16, 0x02, 0xd0, 0x02, 0xa2, 0x0f, 0x8e, 0x14, 0x02, 0x8d, 0x16, 0x02, 0xc9, 0x21, // FD60
			0x30, 0x5e, 0xc9, 0x5f, 0xf0, 0x5a, 0xa9, 0x01, 0x20, 0xbe, 0xfc, 0x20, 0xcf, 0xfc, 0x8d, 0x15, // FD70
			0x02, 0x29, 0x01, 0xaa, 0xad, 0x15, 0x02, 0x29, 0x06, 0xd0, 0x17, 0x2c, 0x13, 0x02, 0x50, 0x2b, // FD80
			0x8a, 0x49, 0x01, 0x29, 0x01, 0xf0, 0x24, 0xa9, 0x20, 0x2c, 0x15, 0x02, 0x50, 0x25, 0xa9, 0xc0, // FD90
			0xd0, 0x21, 0x2c, 0x13, 0x02, 0x50, 0x03, 0x8a, 0xf0, 0x11, 0xac, 0x13, 0x02, 0xc0, 0x31, 0x90, // FDA0
			0x08, 0xc0, 0x3c, 0xb0, 0x04, 0xa9, 0xf0, 0xd0, 0x02, 0xa9, 0x10, 0x2c, 0x15, 0x02, 0x50, 0x03, // FDB0
			0x18, 0x69, 0xc0, 0x18, 0x6d, 0x13, 0x02, 0x29, 0x7f, 0x2c, 0x15, 0x02, 0x10, 0x02, 0x09, 0x80, // FDC0
			0x8d, 0x15, 0x02, 0x68, 0xa8, 0x68, 0xaa, 0xad, 0x15, 0x02, 0x60, 0x20, 0xf9, 0xfe, 0xe6, 0xe4, // FDD0
			0xd0, 0x02, 0xe6, 0xe5, 0xb1, 0xfe, 0x91, 0xe4, 0x20, 0xeb, 0xfb, 0x90, 0xee, 0x60, 0x18, 0xa9, // FDE0
			0x40, 0x7d, 0x28, 0x02, 0x9d, 0x28, 0x02, 0xa9, 0x00, 0x7d, 0x29, 0x02, 0x9d, 0x29, 0x02, 0x60, // FDF0
			0xa2, 0x28, 0x9a, 0xd8, 0x20, 0xa6, 0xfc, 0x20, 0x40, 0xfe, 0xea, 0xea, 0x20, 0x59, 0xfe, 0x8d, // FE00
			0x01, 0x02, 0x84, 0xfe, 0x84, 0xff, 0x4c, 0x7e, 0xf9, 0xae, 0x2f, 0x02, 0xf0, 0x04, 0xce, 0x2f, // FE10
			0x02, 0x60, 0xae, 0x22, 0x02, 0x8e, 0x2f, 0x02, 0x38, 0xad, 0x31, 0x02, 0xe9, 0x40, 0x8d, 0x31, // FE20
			0x02, 0xad, 0x32, 0x02, 0xe9, 0x00, 0xc9, 0xcf, 0xd0, 0x02, 0xa9, 0xd7, 0x8d, 0x32, 0x02, 0x60, // FE30
			0xa0, 0x1c, 0xb9, 0xb2, 0xfb, 0x99, 0x18, 0x02, 0x88, 0x10, 0xf7, 0xa0, 0x07, 0xa9, 0x00, 0x8d, // FE40
			0x12, 0x02, 0x99, 0xff, 0x01, 0x88, 0xd0, 0xfa, 0x60, 0xa0, 0x00, 0x84, 0xf9, 0xa9, 0xd0, 0x85, // FE50
			0xfa, 0xa2, 0x08, 0xa9, 0x20, 0x91, 0xf9, 0xc8, 0xd0, 0xfb, 0xe6, 0xfa, 0xca, 0xd0, 0xf6, 0x60, // FE60
			0x48, 0xce, 0x03, 0x02, 0xa9, 0x00, 0x8d, 0x05, 0x02, 0x68, 0x60, 0x48, 0xa9, 0x01, 0xd0, 0xf6, // FE70
			0x20, 0x57, 0xfb, 0x29, 0x7f, 0x60, 0xa0, 0x08, 0x88, 0x0a, 0x90, 0xfc, 0x60, 0x20, 0xe9, 0xfe, // FE80
			0x4c, 0xee, 0xff, 0xc9, 0x30, 0x30, 0x12, 0xc9, 0x3a, 0x30, 0x0b, 0xc9, 0x41, 0x30, 0x0a, 0xc9, // FE90
			0x47, 0x10, 0x06, 0x38, 0xe9, 0x07, 0x29, 0x0f, 0x60, 0xa9, 0x80, 0x60, 0x20, 0xb6, 0xfe, 0xea, // FEA0
			0xea, 0x20, 0xe6, 0xfb, 0xd0, 0x07, 0xa2, 0x03, 0x20, 0xbf, 0xfe, 0xca, 0x2c, 0xa2, 0x00, 0xb5, // FEB0
			0xfc, 0x4a, 0x4a, 0x4a, 0x4a, 0x20, 0xca, 0xfe, 0xb5, 0xfc, 0x29, 0x0f, 0x09, 0x30, 0xc9, 0x3a, // FEC0
			0x30, 0x03, 0x18, 0x69, 0x07, 0x4c, 0xee, 0xff, 0xea, 0xea, 0xa0, 0x04, 0x0a, 0x0a, 0x0a, 0x0a, // FED0
			0x2a, 0x36, 0xf9, 0x36, 0xfa, 0x88, 0xd0, 0xf8, 0x60, 0xa5, 0xfb, 0xd0, 0x93, 0x4c, 0x00, 0xfd, // FEE0
			0xb1, 0xfe, 0x85, 0xfc, 0x4c, 0xbd, 0xfe, 0x91, 0xfe, 0xe6, 0xfe, 0xd0, 0x02, 0xe6, 0xff, 0x60, // FEF0
            0xd8, 0xa2, 0x28, 0x9a, 0x20, 0xa6, 0xfc, 0x20, 0x40, 0xfe, 0x20, 0x59, 0xfe, 0x8c, 0x00, 0x02, // FF00 Reset start here at FF00 (leftmost column!)
			0xb9, 0xea, 0xfc, 0x20, 0xee, 0xff, 0xc8, 0xc0, 0x16, 0xd0, 0xf5, 0x20, 0xeb, 0xff, 0x29, 0xdf, // FF10
			0xc9, 0x44, 0xd0, 0x03, 0x4c, 0x00, 0xfc, 0xc9, 0x4d, 0xd0, 0x03, 0x4c, 0x00, 0xfe, 0xc9, 0x57, // FF20
			0xd0, 0x03, 0x4c, 0x00, 0x00, 0xc9, 0x43, 0xd0, 0xc7, 0x4c, 0x11, 0xbd, 0x50, 0x3b, 0x2f, 0x20, // FF30
			0x5a, 0x41, 0x51, 0x2c, 0x4d, 0x4e, 0x42, 0x56, 0x43, 0x58, 0x4b, 0x4a, 0x48, 0x47, 0x46, 0x44, // FF40
			0x53, 0x49, 0x55, 0x59, 0x54, 0x52, 0x45, 0x57, 0x00, 0x00, 0x0d, 0x0a, 0x4f, 0x4c, 0x2e, 0x00, // FF50
			0x5f, 0x2d, 0x3a, 0x30, 0x39, 0x38, 0x37, 0x36, 0x35, 0x34, 0x33, 0x32, 0x31, 0x20, 0x8c, 0xff, // FF60
			0xa2, 0x00, 0x8e, 0x00, 0x02, 0xae, 0x00, 0x02, 0xa9, 0xbd, 0x8d, 0x2a, 0x02, 0x20, 0x2a, 0x02, // FF70
			0x8d, 0x01, 0x02, 0xa9, 0x9d, 0x8d, 0x2a, 0x02, 0xa9, 0x5f, 0xd0, 0x03, 0xad, 0x01, 0x02, 0xae, // FF80
			0x00, 0x02, 0x4c, 0x2a, 0x02, 0x20, 0x2d, 0xbf, 0x4c, 0x9e, 0xff, 0x20, 0x36, 0xf8, 0x48, 0xad, // FF90
			0x05, 0x02, 0xf0, 0x17, 0x68, 0x20, 0xb1, 0xfc, 0xc9, 0x0d, 0xd0, 0x10, 0x48, 0x8a, 0x48, 0xa2, // FFA0
			0x0a, 0xa9, 0x00, 0x20, 0xb1, 0xfc, 0xca, 0xd0, 0xfa, 0x68, 0xaa, 0x68, 0x60, 0x20, 0xa6, 0xf9, // FFB0
			0x20, 0xe0, 0xfb, 0xa2, 0x03, 0x20, 0xb1, 0xf9, 0xa5, 0xfc, 0xa6, 0xfd, 0x85, 0xe4, 0x86, 0xe5, // FFC0
			0x60, 0xa2, 0x02, 0xbd, 0x22, 0x02, 0x9d, 0x27, 0x02, 0x9d, 0x2a, 0x02, 0xca, 0xd0, 0xf4, 0x60, // FFD0
			0x4d, 0x2f, 0x01, 0xa9, 0x2e, 0x20, 0xee, 0xff, 0x4c, 0xb6, 0xfe, 0x6c, 0x18, 0x02, 0x6c, 0x1a, // FFE0
			0x02, 0x6c, 0x1c, 0x02, 0x6c, 0x1e, 0x02, 0x6c, 0x20, 0x02, 0x30, 0x01, 0x00, 0xff, 0xc0, 0x01, // FFF0 - FFFF
        };

        public byte Read()
        {
            return pData[Address.W - StartsAt.W];
        }

        public bool Write(byte InData)
        {
            return false;
        }
    }
}
