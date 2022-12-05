# OscCore
A performance-oriented OSC ([Open Sound Control](http://opensoundcontrol.org/spec-1_0)) library for Unity

## Versions and Platforms

Releases are checked for compatability with the latest release of these versions, and should work with anything in between.
- **2018.4.x** (LTS)
- **2019.x** (Official release)

Builds are tested on recent versions of Windows, MacOS, Android, and iOS, using both Mono and IL2CPP runtimes. 

It should work on any platform where you can use `System.Net.Sockets`, but it's not tested on Linux or consoles.

## Installation

Download & import the .unitypackage from the [Releases](https://github.com/stella3d/OscCore/releases) page.

I may put this on the [Unity package manager](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@1.8/manual/index.html) eventually, but i haven't yet.

# Usage

## Receiving messages

### Debug Scene

To test if message receiving is working at runtime in general, you can build the `Runtime Receiver Debug` scene to a player, and it will show you the device's local IP along with a log of received messages..

### Using Components

For a completely set up example, please see the scene `Message Receiving Example`, "Osc Input" object.

To start, add a `OscReceiver` component to a GameObject somewhere.  This component handles listening for all messages on a single port.

Then add a message handler component to that object or any of its children. There are different components for each type of message, all found under `Add Component -> OSC -> Input`.  Message handler components exist for the most common types.

After adding the component, specify an [OSC address](http://opensoundcontrol.org/spec-1_0) to receive at, and hook up the [UnityEvent](https://docs.unity3d.com/Manual/UnityEvents.html) you see in the handler to what you want to do.  

Here's what a receiver and a message handler for a float message look like set up.
![OSC Receiver Component](https://raw.githubusercontent.com/stella3d/osccore-doc-images/master/oscreceiver_withfloathandler.png)


Message handling components will automatically find references to an `OscReceiver` in its parent heirarchy and use that if the reference is not explicitly specified.

###### Under the hood

The `OscServer` instance attached to the receiver component is what will actually handle incoming messages - the component is just a wrapper.  You can use `OscServer` directly in your own scripts. It implements `IDisposable` - just `Dispose()` it to close the underlying socket.

The server must have its `Update()` method ticked to handle main thread queued callbacks. `OscReceiver`handles this for you. 

### Using Code

##### Adding address handlers

There are several different kinds of method that can be registered with a server via script.

##### Single method

You can register a single callback, to be executed on the server's background thread immediately when a message is received, by calling `oscServer.TryAddMethod`.

If you have no need to queue a method to be called on the main thread, you probably want this one.
```csharp
class SingleCallbackExample
{
  OscServer Server { get; set; }      // get the server instance from the OscReceiver component or your own code

  void ReadValues(OscMessageValues values)
  {
      // call ReadElement methods here to extract values
  }

  public SingleCallbackExample()
  {
    // add a single callback that reads message values at `/layers/1/opacity`
    Server.TryAddMethod("/layers/1/opacity", ReadValues);
  }
}
```

###### Method pair

You can register a pair of methods to an address by calling `oscServer.TryAddMethodPair`.

These pairs consist of two methods, with the main thread one being optional.

1) Runs on background thread, immediate execution, just like single methods
2) Runs on main thread, queued on the next frame

This is useful for invoking UnityEvents on the main thread, or any other use case that needs a main thread api.
_Read the message values in the first method._

```csharp
class CallbackPairExample
{
  OscServer Server { get; }
  OscActionPair ActionPair { get; set;}

  float MessageValue;

  void ReadValues(OscMessageValues values) 
  {
    MessageValue = values.ReadFloatElement(0);
  }
  
  void MainThreadMethod() 
  {
    // do something with MessageValue on the main thread
  }

  public CallbackPairExample()
  {
    // add a pair of methods for the OSC address "/layers/2/color/red"
    Server.TryAddMethodPair("/layers/2/color/red", ReadValues, MainThreadMethod);
  }
}
```

### Reading Message Values

Reading values from incoming messages is done on a per-element basis, using methods named like `Read<Type>Element(int elementIndex)`.  

Your value-reading methods will probably look something like this.

```csharp
// these methods would be registered as background thread callbacks for an address
int ReadSingleIntMessage(OscMessageValues values)
{
    return values.ReadIntElement(0);
}

int ReadTripleFloatMessage(OscMessageValues values)
{
    float x  = values.ReadFloatElement(0);
    float y  = values.ReadFloatElement(1);
    float z  = values.ReadFloatElement(2);
}

```

Most data types offer an `Unchecked` version of the method that is slightly faster, and still safe to use if you know for sure what data type an element is.
```csharp
double ReadUncheckedDoubleMessage(OscMessageValues values)
{
    return values.ReadFloat64ElementUnchecked(0);
}

```

##### Monitor Callbacks

IF you just want to inspect message, you can add a monitor callback to be able to inspect every incoming message.

A monitor callback is an `Action<BlobString, OscMessageValues>`, where the blob string is the address.  You can look at the [Monitor Window](https://github.com/stella3d/OscCore/blob/master/Editor/MonitorWindow.cs) code for an example.


## Sending Messages

### Using Components

For a completely set up example, please see the scene `Property Sender Example`.

Add an `OscSender` component to a GameObject somewhere via the `Add Component` menu, `OSC/OSC Sender`.   
This component handles the connection to a single OSC endpoint (IP address + port).

Then add a `Property Output` component via the `Add Component` menu, `OSC/Property Output`.  If you create it as a sibling or child of the `OscSender` component, it will automatically detect it.  If not, assign the "Sender" reference on it.

This component lets you pick any public property of a Unity object and automatically send an OSC message with its value when it changes.
To do so:

1. Assign the GameObject that holds the component you want to get a value from
2. Pick the component via dropdown
3. Pick the property via dropdown

It will look like this filled out.

![Property Output Commponent UI](https://raw.githubusercontent.com/stella3d/osccore-doc-images/master/propertyoutput_filled_single.png)


There is special support for sending a subset of the elements of a _Vector2_ or _Vector3_.  
Select from the "Vector elements" dropdown to send other combinations of x,y, & z.

![Vector Element Dropdown](https://raw.githubusercontent.com/stella3d/osccore-doc-images/master/propertyoutput_floatfromvector.png)


### Using Code

[OscWriter](https://github.com/stella3d/OscCore/blob/master/Runtime/Scripts/OscWriter.cs) handles serialization of individual message elements.

[OscClient](https://github.com/stella3d/OscCore/blob/master/Runtime/Scripts/OscClient.cs) wraps a writer and sends whole messages.

Sending of complex messages with multiple elements hasn't been abstracted yet - take a look at the methods in `OscClient` to see how to send any message you need.

```csharp

OscClient Client = new OscClient("127.0.0.1", 7000);

// single a single float element message
Client.Send("/layers/1/opacity", 0.5f);

// send a blob
byte[] Blob = new byte[256];
Client.Send("/blobs", Blob, Blob.Length);

// send a string
Client.Send("/layers/3/name", "Textural");
```

#### Additional safety checks

Define `OSCCORE_SAFETY_CHECKS` in your project to have reads of message elements be bounds-checked.  It amounts to making sure the element you asked for isn't beyond the number of elements in the message.  

## Protocol Support Details

All OSC 1.0 types, required and non-standard are supported.  

The notable parts missing from [the spec](http://opensoundcontrol.org/spec-1_0) for the initial release are:
- **Matching incoming Address Patterns**

  "_A received OSC Message must be disptched to every OSC method in the current OSC Address Space whose OSC Address matches the OSC Message's OSC Address Pattern"_
   
   Currently, an exact address match is required for incoming messages.
   If our address space has two methods:
   - `/layer/1/opacity`
   - `/layer/2/opacity`

  and we get a message at `/layer/?/opacity`, we _should_ invoke both messages.

  Right now, we would not invoke either message.  We would only invoke messages received at exactly one of the two addresses.
  This is the first thing that i think will be implemented after initial release - some of the other packages also lack this feature, and you can get far without it.

- **Syncing to a source of absolute time**

  "_An OSC server must have access to a representation of the correct current absolute time_". 

  I've implemented this, as a class that syncs to an external NTP server, but without solving clock sync i'm not ready to add it.

- **Respecting bundle timestamps**

  "_If the time represented by the OSC Time Tag is before or equal to the current time, the OSC Server should invoke the methods immediately... Otherwise the OSC Time Tag represents a time in the future, and the OSC server must store the OSC Bundle until the specified time and then invoke the appropriate OSC Methods._"

  This is simple enough to implement, but without a mechanism for clock synchronization, it could easily lead to errors.  If the sending application worked from a different time source than OscCore, events would happen at the wrong time.

## Performance Details

##### Strings and Addresses

Every OSC message starts with an "address", specified as an ascii string.  
It's perfectly reasonable to represent this address in C# as a standard `string`, which is how other libraries work.

However, because strings in C# are immutable & UTF16, every time we receive a message from the network, this now requires us to allocate a new `string`, and in the process expand the received ascii string's bytes (where each character is a single byte) to UTF16 (each `char` is two bytes).   

OscCore eliminates both 
- string allocation
- the need to convert ascii bytes to UTF16

This works through leveraging the [BlobHandles](https://github.com/stella3d/BlobHandles) package.  
Incoming message's addresses are matched directly against their unmanaged ascii bytes.  

This has two benefits 
- no memory is allocated when a message is received
- takes less CPU time to parse a message

#### Why Another OSC Library ?

There are at least 4 other OSC implementations for Unity:
- [OscJack](https://github.com/keijiro/OscJack), which i was using before
- [ExtOSC](https://github.com/Iam1337/extOSC), [UnityOSC](https://github.com/jorgegarcia/UnityOSC), & [OscSimpl](https://assetstore.unity.com/packages/tools/input-management/osc-simpl-53710)

OscCore was created largely because all of these other libraries _allocate memory for each received message_, which will cause lots of garbage collections when used with a large amount of messages.  For more on this see [performance details](#performance-details).

OscCore aims foremost to be **small** & **fast**, so that other systems can be built on top of it. 

One of the existing libraries may satisfy your needs if you don't need to care much about garbage allocation / performance!
