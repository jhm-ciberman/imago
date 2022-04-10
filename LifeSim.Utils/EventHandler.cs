namespace LifeSim;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public delegate void EventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e);