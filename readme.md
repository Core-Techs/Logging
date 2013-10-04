# Core Techs Logging

## Features

- Lightweight 
- Simple API
- No dependencies
- Asynchronous by default
- Logging exceptions won't crash your app 

## Getting Started

### 1. Create a `LogManager` instance

The log manager should be a long lived object in your application. It's responsible for coordinating `LogEntry`'s and `Target`'s.

The preferred way to create and initialize the `LogManager` is to use the `Configure` method:

	// LogManager.Global is just a place for your application wide LogManager
	// but you can store and access it anyway you like
	LogManager.Global = LogManager.Configure("logging");


And in the app.config:


	<configSections>
		<section name="logging" type="CoreTechs.Logging.Configuration.ConfigSection, CoreTechs.Logging" />
	</configSections>
	<logging>
		<!-- logging targets are defined here -->
		<target type="Console" />
	</logging>


Notice the name of the configuration section is passed into the `Configure` method.

You could also configure the LogManager in the application code, if you're into that:

    var console = new ConsoleTarget();
    LogManager.Global = new LogManager(new[] {console});

### 2. Create a `Logger` instance

When you want to log something, you'll need a `Logger` instance.
Typically, you'll create one as a static field in each class that writes to the log:

	// create a logger with the same name as the current class
    private static readonly Logger Log = LogManager.Global.CreateLogger();

The logger's name ends up being the `Source` property of each written log entry. 
You can name the logger anything you like:

	var logger = new Logger(LogManager.Global, "My happy logger");

### 3. Write to the log

Use the `Logger` instance (referenced by `Log` below) to write to the log targets:

	Log.Trace("A small detail");
	Log.Debug("Something {0} is going on.", "fishy");
	Log.Data("Username", "roverby")
		.Data("Email", "roverby@core-techs.net")
		.Info("A user has logged into the system.");

	if (TooManyIncorrectLoginAttempts)
		Log.Data("Username", username)
			.Warn("A user may be trying to break into the system.");

	try
	{
		SomethingDangerous();
	}
	catch (TolerableException ex)
	{
		Log.Exception(ex).Error();
	}
	catch (Exception ex)
	{
		Log.Exception(ex).Fatal();
		throw;
	}

## Targets

	throw new NotImplementedException();
	