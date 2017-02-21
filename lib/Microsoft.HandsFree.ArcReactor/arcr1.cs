// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using System;
using System.Threading.Tasks;
using Microsoft.HandsFree.ArcReactor;

namespace Reflecta.Interfaces
{
    public class arcr1 : ReflectaInterfaceBase
    {
        // This interface name *must* match the interface name in the associated arduino code
        public const string InterfaceName = "arcr1";

        private enum functionOffset : byte
        {
            SetLedState    = 0x00,
            GetLedState    = 0x01,
            ShowImage      = 0x02,
            ShowImageColor = 0x03,
            ShowAnimation  = 0x04,
            ShowPixel      = 0x05,
            ShowPixelColor = 0x06
        }

        public async Task<ArcReactorState> setLedState(ArcReactorState state)
        {
            var functionId = (byte)(_interfaceOffset + functionOffset.SetLedState);
            var value = await _client.SendFrameAndAwaitResponse(new byte[] { (byte)ReflectaClient.FunctionId.PushArray, 1, (byte)state, functionId, (byte)ReflectaClient.FunctionId.SendResponse });

            var newState = (ArcReactorState)value[0];

            return newState;
        }

        public async Task<ArcReactorState> getLedState()
        {
            var functionId = (byte)(_interfaceOffset + functionOffset.GetLedState);
            var value = await _client.SendFrameAndAwaitResponse(new byte[] { (byte)ReflectaClient.FunctionId.PushArray, 0, functionId, (byte)ReflectaClient.FunctionId.SendResponse });

            var newState = (ArcReactorState)value[0];

            return newState;
        }

        public async Task<ArcReactorState> showImage(uint[] pixels)
        {
            var functionId = (byte)(_interfaceOffset + functionOffset.ShowImage);

            var bufferIdx = 0;
            var buffer = new byte[2 + (pixels.Length * 4) + 2];

            buffer[bufferIdx++] = (byte) ReflectaClient.FunctionId.PushArray;
            buffer[bufferIdx++] = (byte)(pixels.Length * 4);

            foreach (var pixel in pixels)
            {
                var data = BitConverter.GetBytes(pixel);

                // need to push as little endian
                buffer[bufferIdx++] = data[3];
                buffer[bufferIdx++] = data[2];
                buffer[bufferIdx++] = data[1];
                buffer[bufferIdx++] = data[0];
            }

            buffer[bufferIdx++] = functionId;
            buffer[bufferIdx] = (byte) ReflectaClient.FunctionId.SendResponse;
            
            var value = await _client.SendFrameAndAwaitResponse(buffer);

            var newState = (ArcReactorState)value[0];

            return newState;
        }

        public async Task<ArcReactorState> showImageColor(uint[] pixels)
        {
            var functionId = (byte)(_interfaceOffset + functionOffset.ShowImageColor);

            var bufferIdx = 0;
            var buffer = new byte[2 + (pixels.Length * 3) + 2];

            buffer[bufferIdx++] = (byte)ReflectaClient.FunctionId.PushArray;
            buffer[bufferIdx++] = (byte)(pixels.Length * 3);

            foreach (var pixel in pixels)
            {
                var pixelRGB = NeoPixelColor.FromRGB888(pixel);

                buffer[bufferIdx++] = pixelRGB.Red;
                buffer[bufferIdx++] = pixelRGB.Green;
                buffer[bufferIdx++] = pixelRGB.Blue;
            }

            buffer[bufferIdx++] = functionId;
            buffer[bufferIdx] = (byte)ReflectaClient.FunctionId.SendResponse;

            var value = await _client.SendFrameAndAwaitResponse(buffer);

            var newState = (ArcReactorState)value[0];

            return newState;
        }

        public async Task<ArcReactorState> showPixel(byte index, uint pixel)
        {
            var data = BitConverter.GetBytes(pixel);

            var functionId = (byte)(_interfaceOffset + functionOffset.ShowPixel);
            var value = await _client.SendFrameAndAwaitResponse(new byte[] { (byte)ReflectaClient.FunctionId.PushArray, 5, index, data[3], data[2], data[1], data[0], functionId, (byte)ReflectaClient.FunctionId.SendResponse });

            var newState = (ArcReactorState)value[0];

            return newState;
        }

        public async Task<ArcReactorState> showPixelColor(byte index, byte red, byte green, byte blue)
        {
            var functionId = (byte)(_interfaceOffset + functionOffset.ShowPixelColor);
            var value = await _client.SendFrameAndAwaitResponse(new byte[] { (byte)ReflectaClient.FunctionId.PushArray, 4, index, red, green, blue, functionId, (byte)ReflectaClient.FunctionId.SendResponse });

            var newState = (ArcReactorState)value[0];

            return newState;
        }

        public async Task<ArcReactorState> showAnimation(byte animationIndex)
        {
            var functionId = (byte)(_interfaceOffset + functionOffset.ShowAnimation);
            var value = await _client.SendFrameAndAwaitResponse(new byte[] { (byte)ReflectaClient.FunctionId.PushArray, 1, animationIndex, functionId, (byte)ReflectaClient.FunctionId.SendResponse });

            var newState = (ArcReactorState)value[0];

            return newState;
        }
    }
}
