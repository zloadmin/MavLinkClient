## Файлы
`FlyDron.cs`

Старый скрипт который конектится к px4 серверу по udp и пытается взлететь дроном. 
Раньше назывался Program.cs 
Там были изменения, оригинал можно взять по комиту https://github.com/zloadmin/MavLinkNet/tree/af64f169de3433d83811b771c04c7f432f012ab2

`1min_hex.log`

Это 10 сообщений (107 HIL_SENSOR) которые gazebo шлет на px4, он используется как тестовые данные для отправки  

`test-messages.txt`

Первые несколько HEX обмена сообщениями между px4 и gazebo


## Виртуальная машина
Настройки порта Gazebo

`~/catkin_ws/src/clover/clover_description/urdf/clover/clover4_gazebo.xacro` Параметр `<mavlink_tcp_port>5555</mavlink_tcp_port>`


## Работа скрипта

1 Инициализируем TCP Server и подписываемся на получения пакетов ```OnPacketReceived```
```
_tcpServer = new MavLinkTcpServerTransport();
_tcpServer.OnPacketReceived += OnPacketReceived;
_tcpServer.Initialize();
```
2 Метод ```_tcpServer.Initialize();``` ждет подключения TCP клиент

3 Ждем первые 2 пакета от px4 (смотри ниже начало обмена)
```
while (firstCommandLong == null && firstHeartbeat == null){}
```
4 Создаем переменую ```UInt64 TimeUsec = 0;``` она нужна будет нам для всех пакетов. Мы будем ее инкриментить на 4000, так подсмотрел у gazebo

5 Начинаем отправку пакетов с интервалом 5 миллисекунд
```
while (true)
{
    _tcpServer.SendMessage(); // <- Шлем нужное нам сообщение 
    TimeUsec += 4000;
    Thread.Sleep(5);
}  
```
6 Очень важные нюансы без которых может ничего не работать. 
* Таймаут должен быть 5 миллисекунд, так шлет и сам px4, его можно немного увеличить, но вот сильно большой или сильно маленький таймаут может все сломать
* Нули - если px4 отвечает нулями Hil_Actuator_Controls это не значит что все работает правильно, нужно всегда проверять на не пустых данных
* TODO: дополнить
7 Очень важные нюансы ДЛЯ ПАКЕТА без которых может ничего не работать.
* TimeUsec - он должен быть обязательно, и он обязательно должен увеличиваться. И нет, это не таймстамп как пишут о нем в доке, в gazebo и jMAVSim это просто число со старта сервера/сркипта
> Timestamp (UNIX Epoch time or time since system boot). The receiving end can infer timestamp format (since 1.1.1970 or since system boot) by checking for the magnitude of the number.
* FieldsUpdated - он заполняется функцией ```ComputeUpdatedMask``` По сути он шлет bitmap на те поля, которые являются валидными. Если эта функция работает не верно, px4 отдает нули всегда. 

## Работа gazebo и отличие от скрипта

**1 Gazebo режит пакеты, но видимо px4 это не тревожит (НО он это указывет)**

Вот пакет gazebo hil_sensor. Указано что его размер 0x3E (62), что для сообщения 0x6b0000(107) не верно, должно быть 64.

FD3E00002901C86B0000E07B08000000000097A6BABC77FBA83D3F693FBDF0CC213BB772443A915CBC3A24D7623CBD5D58BECEA4DC3E15FD6E44FFFF0000BBFDF34320433D41FF1BD9CF

А вот мое сообщение. Его размер 0x40(64) что верно.  8 (time_usec) + (13 * 4) (floats) + fields_updated (4) = 8 + (13 * 4) + 4 = 64

fd40000002ff006b0000d3b00a710000000097a6babc77fba83d3f693fbdf0cc213bb772443a915cbc3a24d7623cbd5d58becea4dc3e15fd6e44ffff0000bbfdf34320433d41ff1b0000f12e

Но не все. Вот пример 115

gazebo

FD4000002A01C8730000E07B0800000000002905353FE44E271EF662279E2905353F8027F09A5F24C7A266759C984C52401C44F41705DE72070000000000A4000000A400000000004E260F2B

мой

fd4000000001c87300006b578871000000002905353fe44e271ef662279e2905353f8027f09a5f24c7a266759c984c52401c44f41705de72070000000000a4000000a400000000004e260760

Оба 0x40 (64)

**2 Gazebo шлет 2 сообщения в одну строку. Мы можем так же делать функцией `MavLinkTcpServerTransport::SendMavlinkMessages` она принимает лист сообщений. Я ее не тестировал, так как px4 достаточно одного типа сообщения, в дальнейшем скорей всего их нужно будет склеивать так же.**






## Работа gazebo начало обмена пакетов. Первые 5 пакетов (test-messages.txt)
[px4 → gazebo] Message ID: 76 (CommandLong)

Шлет команду установить таймаут 5000

FD1E00000001014C00000000E64200409C450000000000000000000000000000000000000000FF011904

```
==================================================
MAVLINK MESSAGE
  Type       : UasCommandLong
  Message ID : 76
  System ID  : 255
  Component  : 0
--------------------------------------------------
Param1 — Parameter 1 (for the specific command).
    value: 115
Param2 — Parameter 2 (for the specific command).
    value: 5000
Param3 — Parameter 3 (for the specific command).
    value: 0
Param4 — Parameter 4 (for the specific command).
    value: 0
Param5 — Parameter 5 (for the specific command).
    value: 0
Param6 — Parameter 6 (for the specific command).
    value: 0
Param7 — Parameter 7 (for the specific command).
    value: 0
Command — Command ID (of command to send).
    value: 511
    enum : MavCmd.SetMessageInterval
    desc : Set the interval between messages for a particular MAVLink message ID. This interface replaces REQUEST_DATA_STREAM
    params:
      - The MAVLink message ID
      - The interval between two messages, in microseconds. Set to -1 to disable and 0 to request default rate.
TargetSystem — System which should execute the command
    value: 0
TargetComponent — Component which should execute the command, 0 for all components
    value: 0
Confirmation — 0: First transmission of this command. 1-255: Confirmation transmissions (e.g. for kill command)
    value: 0
==================================================

```

[px4 → gazebo] Message ID: 0 (Heartbeat)

Шлет Heartbeat

FD09000001010100000000000000000C0000039158

[px4 → gazebo] Message ID: 93 (HilActuatorControls)

Шлет текущие положение - все нули

FD5100000701015D000040E309000000000001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001E156
```
==================================================
MAVLINK MESSAGE
  Type       : UasHilActuatorControls
  Message ID : 93
  System ID  : 255
  Component  : 0
--------------------------------------------------
TimeUsec — Timestamp (UNIX Epoch time or time since system boot). The receiving end can infer timestamp format (since 1.1.1970 or since system boot) by checking for the magnitude the number.
    value: 648000
Flags — Flags as bitfield, reserved for future use.
    value: 1
Controls — Control outputs -1 .. 1. Channel assignment depends on the simulated hardware.
    value[16]: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
Mode — System mode. Includes arming state.
    value: 1
    enum : MavMode.<UNKNOWN>
==================================================

```

[gazebo → px4] Message ID: 107 (HilSensor)

Шлет показатели Hil_Sensor

FD3E00002901C86B0000E07B08000000000097A6BABC77FBA83D3F693FBDF0CC213BB772443A915CBC3A24D7623CBD5D58BECEA4DC3E15FD6E44FFFF0000BBFDF34320433D41FF1BD9CF
```
==================================================
MAVLINK MESSAGE
  Type       : UasHilSensor
  Message ID : 107
  System ID  : 255
  Component  : 0
--------------------------------------------------
TimeUsec — Timestamp (UNIX Epoch time or time since system boot). The receiving end can infer timestamp format (since 1.1.1970 or since system boot) by checking for the magnitude the number.
    value: 556000
Xacc — X acceleration
    value: -0,022784514
Yacc — Y acceleration
    value: 0,08251088
Zacc — Z acceleration
    value: -0,04673123
Xgyro — Angular speed around X axis in body frame
    value: 0,0024688803
Ygyro — Angular speed around Y axis in body frame
    value: 0,00074939005
Zgyro — Angular speed around Z axis in body frame
    value: 0,0014370849
Xmag — X Magnetic field
    value: 0,013845239
Ymag — Y Magnetic field
    value: -0,21129508
Zmag — Z Magnetic field
    value: 0,43094486
AbsPressure — Absolute pressure
    value: 955,9544
DiffPressure — Differential pressure (airspeed)
    value: 9,1834E-41
PressureAlt — Altitude calculated from pressure
    value: 487,98227
Temperature — Temperature
    value: 11,828888
FieldsUpdated — Bitmap for fields that have updated since last message, bit 0 = xacc, bit 12: temperature, bit 31: full reset of attitude/position/velocities/etc was performed in sim.
    value: 7167
==================================================
```

[gazebo → px4] Message ID: 115 (HilStateQuaternion)

Шлет показатели HilStateQuaternion

FD4000002A01C8730000E07B0800000000002905353FE44E271EF662279E2905353F8027F09A5F24C7A266759C984C52401C44F41705DE72070000000000A4000000A400000000004E260F2B

```
==================================================
MAVLINK MESSAGE
  Type       : UasHilStateQuaternion
  Message ID : 115
  System ID  : 255
  Component  : 0
--------------------------------------------------
TimeUsec — Timestamp (UNIX Epoch time or time since system boot). The receiving end can infer timestamp format (since 1.1.1970 or since system boot) by checking for the magnitude the number.
    value: 556000
AttitudeQuaternion — Vehicle attitude expressed as normalized quaternion in w, x, y, z order (with 1 0 0 0 being the null-rotation)
    value[4]: [0,70711, 8,8572206E-21, -8,861371E-21, 0,70711]
Rollspeed — Body frame roll / phi angular speed
    value: -9,932549E-23
Pitchspeed — Body frame pitch / theta angular speed
    value: -5,3977567E-18
Yawspeed — Body frame yaw / psi angular speed
    value: -4,0443597E-24
Lat — Latitude
    value: 473977420
Lon — Longitude
    value: 85455940
Alt — Altitude
    value: 488158
Vx — Ground X Speed (Latitude)
    value: 0
Vy — Ground Y Speed (Longitude)
    value: 0
Vz — Ground Z Speed (Altitude)
    value: 164
IndAirspeed — Indicated airspeed
    value: 0
TrueAirspeed — True airspeed
    value: 164
Xacc — X acceleration
    value: 0
Yacc — Y acceleration
    value: 0
Zacc — Z acceleration
    value: 9806
==================================================
```
## Дополнительные репозитории

`git@github.com:zloadmin/ProxyTCPPX4Gazebo.git`

Это TCP прокси который встает между px4 и gazebo

Он выводит просто листинг hex, я его обычно перенаправляю в файл для дальнейшей обработки

Чем он может помочь
* по нему можно понять с какой частотой идут те или иные пакеты
* hex пакеты можно десерилизовать и использовать данные для примера
* через него в дальнейшем можно будет тестировать саму реализацию сима. Например, пытаться взлететь на дроне, и смотреть по hex кто какие данные передает, а потом пытаться повторить это в симе
* через него вообще можно находить много багов в данных.  через него я понял что например px4 может слать нули в ответ, хотя в дампе хексов это не так. Через него я тестировал функцию ComputeUpdatedMask, без нее px4 может вообще не отвечать. по ней отдельно распишу. и тд.


Вообще симуляция в px4 работает так - px4 сервер стартует, и пытается подключиться к tcp серверу, по порту 4560 и начинает обмен пакетам.
На виртуалке перед запуском px4 запускается gazebo TCP server на порту 4560.

Как работает скрипт. 

* в виртуалке меняем порт на 5555, на котором будет запущен сервер gazebo. Настройки порта лежат в файле `~/catkin_ws/src/clover/clover_description/urdf/clover/clover4_gazebo.xacro` Параметр `<mavlink_tcp_port>5555</mavlink_tcp_port>`
* запускам сам скрипт который поднимет tcp сервер на порту 4560 и будет слушать порт 5555 в качестве tcp клиента
* стартуем клевер
* все данные обмена между px4 и gazebo будут выводиться в консоли, но лучше их направить в файл. `dotnet run > dump.txt`



`git@github.com:zloadmin/PX4TCPSender.git`

Это максимально простой скрип который берет `dump.txt` (листинг hex) и шлет их на tcp gjhn 4560

Как я использую его

* можно просто в основном скрипте, в функции OnPacketReceived вставить код `UasMessagePrinter.Print(packet.Message);` и тогда мы будем просматривать весь дамп в консоле. Сообщения будут распарсены и выведены со всеми показателями
* можно сохранять дамп сообщений в json формате списком. В функцию OnPacketReceived нужно вставить код `File.AppendAllText("json_dump.txt"JsonSerializer.Serialize((UasHilSensor)packet.Message) + Environment.NewLine);` для `packet.MessageId == 107` Этот дамп можно просматривать в читаемом формате, или использовать как фейковые данные 