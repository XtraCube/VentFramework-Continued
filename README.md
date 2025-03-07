# VentFramework (Continued)

## *Note: This repository is the OFFICIAL continuation of Vent Framework due to the original creator passing the project.*

# Information

VentFramework is a library built for Among Us, inspired by the popular library Reactor.

Features:
- Highly robust option system aiming to replace BepIn's builtin configuration
    - Out-of-the-box integration with Among Us's game options
    - Highly configurable rendering for the game options
    - Built-in preset manager for options to allow players to group specific options
- Brand-new localization system that emphasizes easy usability and organization
    - Translations are obtainable statically via attributes or a static method call
- Easy command manager for Among Us chat commands
    - Decorate classes with the command prefix then allow the framework to do all the heavy-lifting
- Super configurable modded handshake support
    - Configure the behavior when modded clients join (Kick, Disable RPC, do nothing)
    - Option syncing between modded clients if your mod deems their version as valid
- Custom RPC attributes with cleaner developmental interface than Reactor
    - Batch interface which allows for objects to be sent across multiple packets
- Loads of new utilities
    - New data structures
        - Batch collection (for automatic sending list of objects with Batch-RPC)
        - An array of "Optional" objects highly influenced by Java Optional API
        - A bunch more
    - Revamped asynchronous methods for running & scheduling code asynchronously
    - Profiling utilities


# Getting Started
Resources for using the framework:
1. [Installation Setup](#installation)
2. [Project Setup](#project-setup)
3. [Custom RPC](#custom-rpc)
4. [Localization](#localization)

# Installation
Whether you are installing VentFramework to use with a mod, or developing with it, the process is the same.
<br/>Make sure these are downloaded and installed in a folder before continuing:
- [Among Us](https://www.innersloth.com/games/among-us/) - Base Game
- [BepInEx #697](https://builds.bepinex.dev/projects/bepinex_be) - Required for C# code Injection (without this, the game isn't modded)

Next, you'll need to download VentFramework, which can be found [here](https://github.com/Lotus-AU/VentFramework/releases/latest).<br/>
Mod authors should provide which version of the framework you’ll need. But versions are usually cross-compatible so if confused try downloading the latest.

Once downloaded, move **VentFrameworkContinued.dll** into your Among Us Directory, specifically `BepInEx/Plugins`.<br/>
Once its there, you're good to go!

If you are a developer, make sure to add a reference by nuget package or DLL (preferred).

# Project Setup
Add `BepInDependency` to your main plugin class.
```csharp
using VentLib;

[BepInProcess("AmongUs.exe")]
[BepInDependency(Vents.Id)]
public class MyExample: BasePlugin
```
Additionally, you may add the **VentModFlags** attribute to specify information about your mod.<br/>
If you add the **RequireOnAllClients** flag, then people without the mod can't even find your lobby by inputting the code!

You can also specify a version of your mod. It will be automatically communicated with other clients.<br/>
By default, it will just be `NoVersion`.<br/>
You can either:
- Create versions Manually
- Create versions based on last GitHub commit

We'll cover both ways.

## Manual
This is not as easy to set up as GitHub, but gives you more control as a Developer.
Add the Interface `IVersionEmitter` to your plugin.
```csharp
using VentLib.Version.Git;
using VentLib;

[BepInProcess("AmongUs.exe")]
[BepInDependency(Vents.Id)]
public class MyExample: BasePlugin, IVersionEmitter
```
<br><br/>
Create a class to hold your version.<br/>
This is where you have most control as you can choose what is communicated via rpc.</br>
Here is an example class:
```csharp
using VentLib.Networking;
/// <summary>
/// Version Representing this Mod
/// </summary>
public class ExampleVersion: VentLib.Version.Version
{
    public readonly string Major;
    public readonly string Minor;
    public readonly string Patch;
    
    public ExampleVersion()
    {
        Major = "1";
        Minor = "1";
        Patch = "2";
    }
    
    internal ExampleVersion(MessageReader reader)
    {
        Major = reader.Read<string>();
        Minor = reader.Read<string>();
        Patch = reader.Read<string>();
    }
    
    public override VentLib.Version.Version Read(MessageReader reader) => new ExampleVersion(reader);

    protected override void WriteInfo(MessageWriter writer)
    {
        writer.Write(Major);
        writer.Write(Minor);
        writer.Write(Patch);
    }

    public override string ToSimpleName()
    {
        return $"Example Addon v{Major}.{Minor}.{Patch}";
    }

    public override string ToString() => "ExampleVersion";
}
```
<br><br/>
Create a static Instance and add a **Getter** functon named `Version`.
```csharp
using VentLib.Version;
using VentLib;

[BepInProcess("AmongUs.exe")]
[BepInDependency(Vents.Id)]
public class MyExample: BasePlugin, IVersionEmitter
{
    public readonly ExampleVersion CurrentVersion = new();
    
    public Version Version() => CurrentVersion;
}
```
<br><br/>
VentFramework also allows you to filter through versions and run actions based on them.<br/>
Here is an example:
```csharp
using VentLib.Version;
using VentLib;

[BepInProcess("AmongUs.exe")]
[BepInDependency(Vents.Id)]
public class MyExample: BasePlugin, IVersionEmitter
{
    public readonly ExampleVersion CurrentVersion = new();
    
    public Version Version() => CurrentVersion;
    
    public HandshakeResult HandshakeFilter(Version version)
    {
        if (handshake is NoVersion) return HandshakeResult.FailDoNothing; // Does nothing to the User if vanilla.
        if (handshake is not ExampleVersion exa) return HandshakeResult.DisableRPC; // Disables RPC of the User if they aren't our version.
        if (CurrentVersion.ToSimpleName() != exa.ToSimpleName()) return HandshakeResult.DisableRPC; // They are on different version.
        return HandshakeResult.PassDoNothing;
    }
}
```

## Github
This is the easier way, but you need to have a somewhat understanding of how `git` works.
When committing your final code before release, **add** a tag to the commit that specifies the version.
`git tag -a <tagname> -m "your message"`<br/>
Example: `git tag -a v1.1.2 -m "v1.1.2 Update"`

Future compiles will automatically have your version set to v1.1.2.
Creating tags on the website will not bump your version.

### Setup
Add the Interface `IGitVersionEmitter` to your plugin.
```csharp
using VentLib.Version.Git;
using VentLib;

[BepInProcess("AmongUs.exe")]
[BepInDependency(Vents.Id)]
public class MyExample: BasePlugin, IGitVersionEmitter
```
<br><br/>
Add a GitVersion parameter that automatically initializes.
```csharp
using VentLib.Version.Git;
using VentLib;

[BepInProcess("AmongUs.exe")]
[BepInDependency(Vents.Id)]
public class MyExample: BasePlugin, IGitVersionEmitter
{
    public readonly GitVersion CurrentVersion = new(typeof(MyExample).Assembly);
}
```
<br><br/>
Then, it is the same as adding the HandShake filter function from before.
```csharp
using VentLib.Version.Git;
using VentLib;

[BepInProcess("AmongUs.exe")]
[BepInDependency(Vents.Id)]
public class MyExample: BasePlugin, IGitVersionEmitter
{
    public readonly GitVersion CurrentVersion = new(typeof(MyExample).Assembly);
    
    public GitVersion Version() => CurrentVersion;
    
    public HandshakeResult HandshakeFilter(Version version)
    {
        return HandshakeResult.PassDoNothing;
    }
}
```
<br><br/>
Like before, you can add checks to make sure the version is the same.
```csharp
public HandshakeResult HandshakeFilter(Version version)
{
    if (handshake is NoVersion) return HandshakeResult.FailDoNothing; // Does nothing to the User if they are vanilla.
    if (handshake is not GitVersion git) return HandshakeResult.DisableRPC; // Disables RPC of the User.
    if (CurrentVersion.Sha != git.Sha) return HandshakeResult.DisableRPC; // They are on different commit or another mod.
    return HandshakeResult.PassDoNothing;
}
```
-------------------
However, we still aren't done.<br/>
There are just a few lines of code that we have to do to finish our Version Control.<br/>
Inside the Initializer for your function, we need to register our version.
```csharp
public static VersionControl VersionControl;

public MyExample()
{
    VersionControl versionControl = MyExample.VersionControl = VersionControl.For(this);
}
```
You will want to save an instance to this Version Control as it has useful features like `GetModdedPlayers()`.

You can also add a Postfix for when everything is finalized. Unlike `HandshakeFilter`, this passes in a **PlayerControl** which allows you to enact actions on the player.
```csharp
public static VersionControl VersionControl;

public MyExample()
{
    VersionControl versionControl = MyExample.VersionControl = VersionControl.For(this);
    versionControl.AddVersionReceiver(ReceiveVersion);
}

private static void ReceiveVersion(Version version, PlayerControl player)
{
    if (player == null) return;
    if (version is NoVersion)
    {
        // In my example, we're going to Kick non-modded clients.
        // This can be achieved by returning HandshakeResult.Kick in HandshakeFilter,
        // but I just want an example.
        AmongUsClient.Instance.KickPlayer(player.GetClientId(), false);
    }
}
```

# Custom RPC
For CustomRPCs we will use the `ModRPCAttribute`.<br/>
The ModRPC attribute is the basis for Custom RPCs. This attribute can be put over any method (static or non-static),
and it'll be automatically picked up by the framework for custom rpc use.

> [!Important]
> Non-static methods are only supported in classes implementing type `IRpcInstance`

The following parameters are supported by ModRPC:

* Any native type (bool, int, uint64, string, double, etc.)
* Any type implementing the interface `IRpcSendable`
* Any List<T> where T is any of the above type
* InnerNetObjects (like PlayerControl)
* Additionally, `RpcOptional` parameters are allowed BUT ModRPC does not support ANY ``null`` values passed into the method.

```csharp
public ModRPCAttribute(uint rpc, RpcActors senders, RpcActors receivers, MethodInvocation invocation)
```
- uint rpc: The custom and unique rpc-id to send.
- RpcActors senders: **Default: RpcActors.Everyone** the allowed sender of this RPC.
    - Note: calls to this method from non-allowed senders **ONLY** blocks the RPC from being sent, based on the `MethodInvocation` parameter, this method still may end up running.
- RpcActors receivers: **Default: RpcActors.Everyone** the allowed receiver of this RPC. This rule is handled by the receiving client and NOT the sending client.
- MethodInvocation invocation: If and when the code for the method should be run.
  <br/><br/>

Used in ModRPC attribute to specify allowed senders / receivers of RPC
```csharp
public enum RpcActors
{
    /// Ignores sending / receiving of RPC
    None,
    /// Only allows host to send / receive RPC
    Host,
    /// Permits everyone BUT host to Send / Receive marked RPC
    NonHosts,
    /// Receiver only! Special value which allows for senders to "respond" back to the last client that sent the specific RPC
    LastSender,
    /// Allows any caller to send / receive RPC
    Everyone
}
```

## Example RPC
```csharp
public enum MyRPCs: uint // IMPORANT! (uint)
{
    PublicSendMsg,
    HostSendMsg
}

// Sends / receives a message
[ModRPC((uint)MyRPCs.PublicSendMessage)]
public void AnyoneSendMessage(string message) {
    log.Info($"Message Received: {message}");
}
    
// Allows only host to send a message, and allows for only non-hosts to receive the message
[ModRPC((uint)MyRPCs.HostSendMsg, senders: RpcActors.Host, receivers: RpcActors.NonHost, MethodInvocation.ExecuteBefore]
public void HostMessage(string message) {
    log.Info($"I am not the host and I received this: \"{message}\" message.);
}

AnyoneSendMessage("example"); // Does not send rpc to other clients.
HostMessage("I changed impostor count"); // Sends RPCs to clients AFTER our function runs. If our function gives an error, RPC is never sent.

// Can Also do this:
Vents.FindRPC((uint)MyRPCs.PublicSendMessage).Send(null, "example");
```

# Localization
For localization, you can either use the attribute or manually edit the .yaml file yourself.
`public class LocalizedAttribute : Attribute`

We will be talking about the Attribute.<br/>
Here is a Usage example:
```csharp
using VentLib.Localization.Attributes;

[Localized(nameof(Roles)]
public class Roles
{
    [Localized(nameof(Example)]
    public class Example
    {
        // These values will be automatically converted on game start.
        // It will also be automatically added to your language file.
        [Localized(nameof(RoleName)] public static string RoleName = "Example";
        [Localized(nameof(Description)] public static string Description = "This is an example description.";
    }
}

// There is also a Localizer object.
// It is used when you dont want a class full of translations, and you want to add text manually.
using VentLib.Localization;

Localizer localizer = Localizer.Get(); // You can pass in an Assembly, but it gets the calling one.
localizer.Translate("Roles.Example.RoleName"); // Should return "Example". If it doesnt exist, returns <Roles.Example.RoleName>
```
Please do note that this will only get the string of language set currently, (usually at load time is when you use this).<br/>
You can always postfix `TranslationController.SetLanguage` to update anything necessary. (use a low priority)

### Disclaimer

-----
This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.