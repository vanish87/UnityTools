#pragma once

namespace KlakNDI
{
    // NDI sender class
    class Sender
    {
    public:

        Sender(const char* name)
        {
            NDIlib_send_create_t send(name, nullptr, false);
            instance_ = NDIlib_send_create(&send);
        }

        ~Sender()
        {
            NDIlib_send_destroy(instance_);
        }

        void sendFrame(void* data, int width, int height, uint32_t fourCC)
        {
            static NDIlib_video_frame_v2_t frame;

			auto format = static_cast<NDIlib_FourCC_type_e>(fourCC);
			auto byteStride = width * 4;//default is 4 bytes but uyvy is 2
			
			switch (format)
			{
			case NDIlib_FourCC_type_UYVY:
			case NDIlib_FourCC_type_UYVA:
				byteStride = width * 2;
				break;
			default:
				break;
			}

            frame.xres = width;
            frame.yres = height;
            frame.FourCC = format;
			frame.frame_rate_N = 60;
			frame.frame_rate_D = 1;
            frame.frame_format_type = NDIlib_frame_format_type_progressive;
            frame.p_data = static_cast<uint8_t*>(data);
            frame.line_stride_in_bytes = byteStride;

            NDIlib_send_send_video_async_v2(instance_, &frame);
        }

        void synchronize()
        {
            NDIlib_send_send_video_async_v2(instance_, nullptr);
        }

    private:

        NDIlib_send_instance_t instance_;
    };
}
