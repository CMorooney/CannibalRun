using Godot;
using System.Runtime.CompilerServices;
using System;

public static class Utils
{
    // this would arguably be better as an extension on Node but then
    // I couldn't use these compiler flags which seem useful in this specific case
    public static T GetOrThrow<T>(Node node,
                                  string name,
	                              [CallerMemberName] string callerMemberName = "unknown",
                                  [CallerFilePath] string callerFilePath = "unknown",
                                  [CallerLineNumber] int lineNumber = -1) where T : class
    {
        var it = node.GetNode<T>(name);
        if(it == null)
        { 
            var message = $"{callerFilePath} : {callerMemberName} ({lineNumber})] Couldn't find {typeof(T).FullName} by name of {name}. Exiting.";
            GD.Print(message);
            throw new ApplicationException(message);
        }
        return it;
    }
}
