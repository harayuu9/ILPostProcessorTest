using System;
using UnityEngine;

namespace Hash.Runtime
{

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
sealed class ConstExprAttribute : Attribute
{
}

public static class Hash
{
    public static int CalcHash(string str) => Animator.StringToHash(str);
}
}