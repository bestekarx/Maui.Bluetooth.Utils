using Maui.Bluetooth.Utils.Shared.Interfaces;
using Maui.Bluetooth.Utils.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using TextAlignment = Maui.Bluetooth.Utils.Shared.Models.TextAlignment;

namespace Maui.Bluetooth.Utils.Shared.Services
{
    /// <summary>
    /// Extension methods for registering Bluetooth printer services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Bluetooth printer services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddBluetoothPrinterServices(this IServiceCollection services)
        {
            // Register main Bluetooth printer manager
            services.AddSingleton<BluetoothPrinterManager>();

            // Register platform-specific Bluetooth service
#if ANDROID
            services.AddSingleton<IBluetoothService, Platforms.Android.AndroidBluetoothService>();
#elif IOS
            services.AddSingleton<IBluetoothService, Platforms.iOS.IOSBluetoothService>();
#else
            throw new PlatformNotSupportedException("Bluetooth is only supported on Android and iOS platforms.");
#endif

            // Register printer services
            services.AddTransient<IEscPosPrinterService, EscPosPrinterService>();
            services.AddTransient<IZebraPrinterService, ZebraPrinterService>();
            services.AddTransient<IGenericPrinterService, GenericPrinterService>();
            services.AddTransient<EscPosPrinterService>();
            services.AddTransient<ZebraPrinterService>();
            services.AddTransient<GenericPrinterService>();

            return services;
        }

        /// <summary>
        /// Add Bluetooth printer services with custom configuration
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configure">Configuration action</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddBluetoothPrinterServices(this IServiceCollection services, Action<BluetoothPrinterOptions> configure)
        {
            // Register options
            services.Configure(configure);

            // Register services
            services.AddBluetoothPrinterServices();

            return services;
        }

        /// <summary>
        /// Add Bluetooth printer services with custom Bluetooth service implementation
        /// </summary>
        /// <typeparam name="TBluetoothService">Custom Bluetooth service type</typeparam>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddBluetoothPrinterServices<TBluetoothService>(this IServiceCollection services) 
            where TBluetoothService : class, IBluetoothService
        {
            // Register main Bluetooth printer manager
            services.AddSingleton<BluetoothPrinterManager>();

            // Register custom Bluetooth service
            services.AddSingleton<IBluetoothService, TBluetoothService>();

            // Register printer services
            services.AddTransient<IEscPosPrinterService, EscPosPrinterService>();
            services.AddTransient<IZebraPrinterService, ZebraPrinterService>();
            services.AddTransient<IGenericPrinterService, GenericPrinterService>();
            services.AddTransient<EscPosPrinterService>();
            services.AddTransient<ZebraPrinterService>();
            services.AddTransient<GenericPrinterService>();

            return services;
        }

        /// <summary>
        /// Add Bluetooth printer services with custom printer service implementations
        /// </summary>
        /// <typeparam name="TEscPosService">Custom ESC/POS service type</typeparam>
        /// <typeparam name="TZebraService">Custom Zebra service type</typeparam>
        /// <typeparam name="TGenericService">Custom generic service type</typeparam>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddBluetoothPrinterServices<TEscPosService, TZebraService, TGenericService>(this IServiceCollection services)
            where TEscPosService : class, IEscPosPrinterService
            where TZebraService : class, IZebraPrinterService
            where TGenericService : class, IGenericPrinterService
        {
            // Register main Bluetooth printer manager
            services.AddSingleton<BluetoothPrinterManager>();

            // Register platform-specific Bluetooth service
#if ANDROID
            services.AddSingleton<IBluetoothService, Platforms.Android.AndroidBluetoothService>();
#elif IOS
            services.AddSingleton<IBluetoothService, Platforms.iOS.IOSBluetoothService>();
#else
            throw new PlatformNotSupportedException("Bluetooth is only supported on Android and iOS platforms.");
#endif

            // Register custom printer services
            services.AddTransient<IEscPosPrinterService, TEscPosService>();
            services.AddTransient<IZebraPrinterService, TZebraService>();
            services.AddTransient<IGenericPrinterService, TGenericService>();
            services.AddTransient<EscPosPrinterService>();
            services.AddTransient<ZebraPrinterService>();
            services.AddTransient<GenericPrinterService>();

            return services;
        }
    }

    /// <summary>
    /// Bluetooth printer configuration options
    /// </summary>
    public class BluetoothPrinterOptions
    {
        /// <summary>
        /// Default scan timeout in milliseconds
        /// </summary>
        public int DefaultScanTimeout { get; set; } = 10000;

        /// <summary>
        /// Default connection timeout in milliseconds
        /// </summary>
        public int DefaultConnectionTimeout { get; set; } = 30000;

        /// <summary>
        /// Default read timeout in milliseconds
        /// </summary>
        public int DefaultReadTimeout { get; set; } = 5000;

        /// <summary>
        /// Default write timeout in milliseconds
        /// </summary>
        public int DefaultWriteTimeout { get; set; } = 5000;

        /// <summary>
        /// Whether to auto-detect printer type
        /// </summary>
        public bool AutoDetectPrinterType { get; set; } = true;

        /// <summary>
        /// Default printer type when auto-detection fails
        /// </summary>
        public PrinterType DefaultPrinterType { get; set; } = PrinterType.EscPos;

        /// <summary>
        /// Whether to enable debug logging
        /// </summary>
        public bool EnableDebugLogging { get; set; } = false;

        /// <summary>
        /// Whether to enable connection retry
        /// </summary>
        public bool EnableConnectionRetry { get; set; } = true;

        /// <summary>
        /// Maximum connection retry attempts
        /// </summary>
        public int MaxConnectionRetries { get; set; } = 3;

        /// <summary>
        /// Retry delay in milliseconds
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// Whether to enable print job queuing
        /// </summary>
        public bool EnablePrintJobQueuing { get; set; } = true;

        /// <summary>
        /// Maximum print job queue size
        /// </summary>
        public int MaxPrintJobQueueSize { get; set; } = 100;

        /// <summary>
        /// Whether to enable printer status monitoring
        /// </summary>
        public bool EnablePrinterStatusMonitoring { get; set; } = true;

        /// <summary>
        /// Printer status monitoring interval in milliseconds
        /// </summary>
        public int PrinterStatusMonitoringInterval { get; set; } = 5000;
    }
} 