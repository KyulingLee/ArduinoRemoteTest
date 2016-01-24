using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//아두이노 제어를 위한 참조 추가
using Microsoft.Maker.Serial;
using Microsoft.Maker.RemoteWiring;

//디버그 콘솔에서 확인 가능하도록 하는 라이브러리 추가
using System.Diagnostics;

// 빈 페이지 항목 템플릿은 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 에 문서화되어 있습니다.

namespace ArduinoRemoteTest
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //클래스 변수 추가
        //USB 시리얼 연결 정보와 아두이노 장비에 대한 개체 선언
        UsbSerial connection;
        RemoteDevice arduino;
        
        public MainPage()
        {
            this.InitializeComponent();

            //아두이노와 연결하기 위한 준비. 하드웨어 ID를 이용한다.
            connection = new UsbSerial("VID_2A03", "PID_0043");

            //아두이노 연결하기
            arduino = new RemoteDevice(connection);
            //디바이스 장치 준비. 아두이노의 setup과 같은 역할을 한다.
            arduino.DeviceReady += Arduino_DeviceReady;
            //아날로그 핀 업데이트 동작이 생길 시에 진행되는 함수 설정한다.
            arduino.AnalogPinUpdated += Arduino_AnalogPinUpdated;
            //아두이노 연결 작업이 성립되었을 때의 동작을 설정한다.
            connection.ConnectionEstablished += Connection_ConnectionEstablished;
            //커넥션을 연결한다.
            connection.begin(57600, SerialConfig.SERIAL_8N1);
        }

        //디바이스 장치 준비
        private void Arduino_DeviceReady()
        {
            //아날로그 핀의 정보가 
            arduino.pinMode("A0", PinMode.ANALOG);
        }

        //연결 작업이 성립되었을 때의 동작
        private void Connection_ConnectionEstablished()
        {
            //on off 버튼이 장치 제어와 연결될 수 있도록 디스패쳐에 추가.
            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
            {
                onButton.IsEnabled = true;
                offButton.IsEnabled = true;
            }));
            
        }        

        //아날로그 핀 업데이트 되었을 때 일어날 동작
        private void Arduino_AnalogPinUpdated(byte pin, ushort value)
        {
            //아날로그 동시 제어를 위해 C#에서 디바이스용 쓰레드를 이용.
            //별도의 쓰레드 충돌을 막기 위해 빈 Dispatcher를 이용해준다.
            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
            {
                UInt16 val = arduino.analogRead("A0");
                analogDataTextbox.Text = val.ToString();
            }));
        }

        //LED 켜는 동작
        private void onButton_Click(object sender, RoutedEventArgs e)
        {
            arduino.digitalWrite(13, PinState.HIGH);
        }

        //LED 끄는 동작
        private void offButton_Click(object sender, RoutedEventArgs e)
        {
            arduino.digitalWrite(13, PinState.LOW);
        }
    }
}
