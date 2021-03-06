using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Test
{
public class Simple : MonoBehaviour
{
    private void Start()
    {
        var sw = new Stopwatch();
        sw.Start();

        for (var i = 0; i < 10000000; i++)
        {
            Hash.Runtime.Hash.CalcHash("少し長い単語のハッシュ値を計算して本当に早いかどうか確かめたいと思っているのですがいかがでしょうか10");
        }
        sw.Stop();
        Debug.Log("constexpr hash:" + sw.ElapsedMilliseconds + " hash:" + Hash.Runtime.Hash.CalcHash("少し長い単語のハッシュ値を計算して本当に早いかどうか確かめたいと思っているのですがいかがでしょうか10"));

        var       str = "少し長い単語のハッシュ値を計算して本当に早いかどうか確かめたいと思っているのですがいかがでしょうか";
        const int cnt = 10;
        str += cnt;
        sw.Restart();
        for (var i = 0; i < 10000000; i++)
        {
            Hash.Runtime.Hash.CalcHash(str);
        }
        sw.Stop();
        Debug.Log("default hash:" + sw.ElapsedMilliseconds + " hash:" + Hash.Runtime.Hash.CalcHash(str));
    }
}
}