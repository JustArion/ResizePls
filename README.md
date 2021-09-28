# ResizePls

A MelonMod that allows changing the Game's windows (MelonConsole Included).

Note: "`SW_HIDE`" is only availble if you use the Game Launch Argument of `--Dawn.AdvancedResize`
In doing this you accept the following responsibility:
```
The User is Advanced enough to know that hiding the application has consequences.
Consequences are:
     The Window is not able to normally be opened again.
     The instance of VRC will still be running and can be seen in Ctrl + Shift + Esc (Task Manager).
```

```ini
[Possible States]
SW_HIDE
SW_SHOWNORMAL
SW_SHOWMINIMIZED
SW_SHOWMAXIMIZED
```


### MelonPreferences.cfg (Default Values)
```ini
[Resizer]
Enabled = true
WindowState = "SW_SHOWNORMAL"
IncludeMelonConsole = false
```

### Credits

Special thanks to [Knah](https://github.com/knah) for IntPtr knowledge!
