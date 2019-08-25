# Ascentis.AsyncDisposer Component

Use this static class to dispose of IDisposable objects using async semantics.
When to use this?
If you use third party software which you can't modify that has caused you trouble when disposing objects on an event which later makes software beyond your control throw exceptions because somewhere in the call stack it tries to use the diposed object.
With this class you can enqueue the IDisposable instance in the AsyncDisposer queue and it will be guaranteed to stay in queue for a specified global period of time before being take for disposal.
Tune the timing in a way that gives time enough for the calling procedure in the stack to complete and return to the caller. 