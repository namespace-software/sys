using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Switcher
{
    protected int _startIndex;

    protected Switcher(int startIndex)
    {
        _startIndex = startIndex;
    }

    public EnumAvailableDirection GetNextDirection(EnumAvailableDirection prevDirection)
    {
        if (prevDirection == EnumAvailableDirection.Forward) _startIndex += 1;
        else if (prevDirection == EnumAvailableDirection.Backward) _startIndex -= 1;

        return GetCurrentAvailableDirection();
    }

    public void ResetToIndex(int index)
    {
        _startIndex = index;
    }

    public abstract EnumAvailableDirection GetCurrentAvailableDirection();
}

public class TwoWaySwitcher : Switcher
{
    public TwoWaySwitcher(int startIndex) : base(startIndex)
    {
    }

    public override EnumAvailableDirection GetCurrentAvailableDirection()
    {
        if (_startIndex < 1) return EnumAvailableDirection.Forward;
        else return EnumAvailableDirection.Backward;
    }

    //public override EnumAvailableDirection GetNextDirection(EnumAvailableDirection prevDirection)
    //{
    //    if (prevDirection == EnumAvailableDirection.Forward) _startIndex += 1;
    //    else if (prevDirection == EnumAvailableDirection.Backward) _startIndex -= 1;

    //    return prevDirection == EnumAvailableDirection.Forward ? EnumAvailableDirection.Backward : EnumAvailableDirection.Forward;
    //}
}

public class ThreeWaySwitcher : Switcher
{
    public ThreeWaySwitcher(int startIndex) : base(startIndex)
    {
    }

    public override EnumAvailableDirection GetCurrentAvailableDirection()
    {
        if (_startIndex < 1) return EnumAvailableDirection.Forward;
        else if (_startIndex > 1) return EnumAvailableDirection.Backward;
        else return EnumAvailableDirection.Both;
    }

    //public override EnumAvailableDirection GetNextDirection(EnumAvailableDirection prevDirection)
    //{
    //    if (prevDirection == EnumAvailableDirection.Forward) _startIndex += 1;
    //    else if (prevDirection == EnumAvailableDirection.Backward) _startIndex -= 1;

    //    return GetCurrentAvailableDirection();
    //}
}

public class FourWaySwitcher : Switcher
{
    public FourWaySwitcher(int startIndex) : base(startIndex)
    {
    }

    public override EnumAvailableDirection GetCurrentAvailableDirection()
    {
        if (_startIndex < 1) return EnumAvailableDirection.Forward;
        else if (_startIndex > 2) return EnumAvailableDirection.Backward;
        else return EnumAvailableDirection.Both;
    }

    //public override EnumAvailableDirection GetNextDirection(EnumAvailableDirection prevDirection)
    //{
    //    if (prevDirection == EnumAvailableDirection.Forward) _startIndex += 1;
    //    else if (prevDirection == EnumAvailableDirection.Backward) _startIndex -= 1;

    //    return GetCurrentAvailableDirection();
    //}
}