# Lab - Push Application to Pivotal Cloud Foundry

## High Level Objectives
* Learn how to deploy an application to Pivotal Cloud Foundry

## Prerequisites
* Ensure you have installed the [Cloud Foundry CLI](https://github.com/cloudfoundry/cli/releases)
* Know the *api* endpoint for the Pivotal Cloud Foundry you are targeting. It will typically look like this: ```https://api.sys.pcf.pcfonazure.com```, and will be referenced in this lab as [PCF-API endpoint]
* Know the Pivotal Cloud Foundry *Apps Manager* endpoint to view the status of your apps in your browser. It will typically look like this: ```https://apps.sys.pcf.pcfonazure.com```, and will be referenced in this lab as [PCF Apps Manager endpoint]
* Know your Pivotal Cloud Foundry username. It will be referenced in this lab as [PCF username]
* Know your Pivotal Cloud Foundry users password. It will be referenced in this lab as [PCF password]

#### Steps
1. We need to modify our code to enable our application to read the listening port from the environment. In a containerized world, we do not want to manage the port our application is listening on. We want our environment to tell us.
  * Add: ```<PackageReference Include="Microsoft.Extensions.Configuration.CommandLine" Version="1.1.2" />``` to the **ItemGroup**. Your final code should look like this:

    ```
    <ItemGroup>
      <PackageReference Include="Microsoft.AspNetCore" Version="1.1.2" />
      <PackageReference Include="Microsoft.AspNetCore.Mvc" Version="1.1.3" />
      <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="1.1.2" />
      <PackageReference Include="System.Runtime.Serialization.Json" Version="4.3.0" />
      <PackageReference Include="Microsoft.Extensions.Configuration.CommandLine" Version="1.1.2" />
    </ItemGroup>
    ```

  * We need to modify **Program.cs** to use the config passed in through the command line. Modify the file to look like this:

    ```
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    namespace weather_retrieval
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var config = new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build();

                var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseConfiguration(config)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build();

                host.Run();
            }
        }
    }
    ```

  * We are also going to setup our project to run on a variety of operating systems. For this lab we will be pushing to Pivotal Cloud Foundry and it's Elastic Runtime (ERT). The ERT containerizes apps using the Ubuntu OS. We could just as well push our .NET Core apps to the Runtime for Windows. Because we will be pushing this app to Ubuntu Linux, we need to define that in the **weather-retrival.csproj** file. Please ensure that the **PropertyGroup** tag, looks like the following:
    ```
    <PropertyGroup>
      <OutputType>Exe</OutputType>
      <TargetFramework>netcoreapp1.1</TargetFramework>
      <RuntimeIdentifiers>win10-x64;osx.10.11-x64;ubuntu.14.04-x64</RuntimeIdentifiers>
    </PropertyGroup>
    ```

  * Restore dependencies

    ```dotnet restore```  


1. In your terminal, set the PCF-API endpoint in the Cloud Foundry CLI

    ```
    cf api [PCF-API endpoint] --skip-ssl-validation
    ```

    i.e.

    ```
    cf api https://api.sys.pcf.pcfonazure.com --skip-ssl-validation
    ```

1. Login to Pivotal Cloud Foundry:

    ```
    cf login
    ```

    Follow the prompts to enter your [PCF username] and [PCF password]. When prompted for your space, select *dev*

1. Login to the Pivotal Cloud Foundry Apps Manager for a GUI view of your applications. In your browser, navigate to: [PCF Apps Manager endpoint]. Enter your [PCF username] and [PCF password] and explore the Apps Manager. Discover:
    * My Account
    * Quotas
    * Orgs and Spaces
    * Marketplace
    * Docs
    * Tools
    * Domain
    * Members
1. Jump back over to the terminal and push your app to Pivotal Cloud Foundry
    * In your terminal navigate to the root of your *weather-retrieval* application that you setup
    * Publish your app. We need to publish a Release version of our app that contains the app as well as all dependencies that will execute on Ubuntu Linux. Run the following at the command line at the root of your project:

      ```
      dotnet publish -r ubuntu.14.04-x64 --configuration Release
      ```

    * There will be a directory with your application in ```/bin/Release/netcoreapp1.1/ubuntu.14.04-x64/publish```. We will target that for the push.
    * Execute the push:

      ```
      cf push weather-retrieval --random-route -i 1 -p ./bin/Release/netcoreapp1.1/ubuntu.14.04-x64/publish/
      ```

      Note: execute ```cf push --help``` with no parameters to see what the parameters for cf push mean. Also, we are using the ```--random-route``` parameter. Pivotal Cloud Foundry will create a route for your app based on the domain of the org and the application name. If you are executing this lab in a classroom with other students, then the first student to execute the push will claim the route and everyone else will error out. Random route ensures each student will have a unique route for their app.
1. Test drive your new app
    * Get the route to your app from the output of the ```cf push```:
      ![alt text](screenshots/cf-push-get-route.png "Route to application")
    * Enter the route in your browser followed by the path of ```/hourlyforecast?latitude=39.7456&longitude=-97.0892```. You should see the same result that you saw locally
    * Think about the things that you didn't have to do:
      * You didn't provision a VM
      * You didn't install an application runtime
      * You didn't deploy an application to a VM or container
      * You didn't configure a load balancer
      * You didn't configure ssl termination
      * You didn't configure a firewall
1. Test drive some other *cf* commands
    * ```cf app weather-retrieval```
    * ```cf scale weather-retrieval -i 2```

      The *scale* command scales your application to 2 instances. In addition to the tasks above that you didn't have to worry about, you also didn't have to reconfigure your load balancer and update routes
    * ```cf events weather-retrieval```
    * ```cf logs weather-retrieval --recent```
    * ```cf restart weather-retrieval```
    * ```cf restage weather-retrieval```
1. View your app in the Apps Manager
    * Switch to your browser and refresh the Apps Manager. You should see your application in the dev space. You will notice that many of the options available to you on the command line are available in the gui. Discover:
      * Routes
      * Logs
      * Settings
      * Scaling
      * Environment Variables
      * Settings
      * Metrics via PCF Metrics (link found in the *Overview* pane)
1. Create a manifest for your application
    * We create manifests to capture the parameters of ```cf push```. We can have different manifests depending on the environment to which we are pushing, or use it to simplify what we have to enter on the command line
    * In your terminal or IDE create a file at the root of the *lab* application named ```manifest.yml```
    * Add the following contents to the file:

      ```
      ---
      applications:
      - name: weather-retrieval
        random-route: true
        memory: 512M
        disk: 1G
        instances: 2
        path: ./bin/Release/netcoreapp1.1/ubuntu.14.04-x64/publish/r
      ```

    * At your command line, make sure you are in the root of the *lab* application, and execute ```cf push```. Note that *cf* found the manifest file and didn't require any command line parameters
