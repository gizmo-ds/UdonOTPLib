using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

public class TOTP_Example : UdonSharpBehaviour
{
  [SerializeField] private TextMeshProUGUI _codeText;
  [SerializeField] private GameObject _placeholder;
  [SerializeField] private Slider _countdown;
  [SerializeField] private UdonOTPLib _udonOTPLib;
  private string _code = "";

  public string secret = "";
  public int digits = 6;
  public int period = 30;
  public Transform _TP_Point;

  public void Enter()
  {
    string code = _udonOTPLib.TOTP(secret, _udonOTPLib.Timestamp(), digits, period);
    if (_code.Equals(code))
    {
      // Authorization allowed
      if (_TP_Point != null)
      {
        Networking.LocalPlayer.TeleportTo(_TP_Point.position, _TP_Point.rotation);
      }
    }
    else
    {
      // Authorization denied
      // do some
    }
    SendCustomEvent("Clear");
  }

  public void Clear()
  {
    _code = "";
    _codeText.text = "";
    _placeholder.SetActive(true);
  }

  public void FixedUpdate()
  {
    _countdown.value = (float)_udonOTPLib.Countdown(period) / period;
  }

  private void _input(string s)
  {
    Debug.Log($"_input: {s}");
    if (_code.Length >= digits) return;
    _placeholder.SetActive(false);
    _code += s;
    _codeText.text = "".PadRight(_code.Length, '●');
  }

  public void Input0() => _input("0");
  public void Input1() => _input("1");
  public void Input2() => _input("2");
  public void Input3() => _input("3");
  public void Input4() => _input("4");
  public void Input5() => _input("5");
  public void Input6() => _input("6");
  public void Input7() => _input("7");
  public void Input8() => _input("8");
  public void Input9() => _input("9");
}
