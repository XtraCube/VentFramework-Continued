using System;

namespace VentLib.Options.Enum;

public enum OptionType
{
    Undefined = -1,
    Bool = 1,
    Int,
    Float,
    String,
    Player, // should really not be used... I won't implement it just yet as I don't see why but maybe some time in the future.
    Title, // will be treated as Undefined
}