# UdonOTPLib

VRChat Udon OTP Library, HOTP([RFC 4226](https://tools.ietf.org/html/rfc4226)) & TOTP([RFC 6238](https://tools.ietf.org/html/rfc6238)).

[Demo world](https://vrchat.com/home/launch?worldId=wrld_1590a2c1-7f17-40b7-a71c-b90b542a204c)

## Requirements

- Unity 2019.4.31f1
- [VRCSDK3](https://vrchat.com/home/download) (Tested version: 3.1.7)
- [UdonSharp](https://github.com/MerlinVR/UdonSharp) (Tested version: 1.1.1)
- [UdonHashLib](https://github.com/Gorialis/vrchat-udon-hashlib) (Tested version: 1.1.0)

## How to use

> Contains a simple example that you can use however you want.

1. Make sure you have imported the latest [VRCSDK3](https://vrchat.com/home/download), [UdonSharp](https://github.com/MerlinVR/UdonSharp) and [UdonHashLib](https://github.com/Gorialis/vrchat-udon-hashlib).
2. Download the latest [release](https://github.com/GizmoOAO/UdonOTPLib/releases/latest) and import it into your project.
3. Open the `_Gizmo/UdonOTPLib/Example/TOTP_Example.unity` scene.
4. You may need to import `TextMeshPro`.
5. Edit the `Secret` variable in the `TOTP_Example` gameobject. (You can use [totp-wasm.vercel.app](https://totp-wasm.vercel.app) to generate `Secret`)
6. enjoy :)

> **Warning**  
> If you don't use [VCC](https://vcc.docs.vrchat.com/), you may not be able to use the latest version of `UdonSharp`. 🤡

### Parameter

| Name   | Type   | Description                  |
| ------ | ------ | ---------------------------- |
| secret | string | Secret key encoded in base32 |
| digits | int    | Code digits                  |
| period | int    | Time period (seconds)        |
| t      | double | Timestamp                    |

### Code example

```cs
string secret = "2LESRALCTRW3B3J4WYSXFQYE5ZR6V5R2";
int digits = 6;
int period = 30;

// Get the code
string code = otp.TOTP(secret, otp.Timestamp(), digits, period);
Debug.Log($"Code: {code}");

// Countdown
int s = otp.Countdown(period);
Debug.Log($"Countdown: {s}sec");
```

## Related

- [totp-wasm](https://github.com/GizmoOAO/totp-wasm)
- [UdonSharp](https://github.com/MerlinVR/UdonSharp)
- [UdonHashLib](https://github.com/Gorialis/vrchat-udon-hashlib)

## License

Code is distributed under [MIT license](./LICENSE), feel free to use it in your proprietary projects as well.
