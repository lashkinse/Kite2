#define TRACE
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NTwain.Data;
using NTwain.Internals;
using NTwain.Interop;
using NTwain.Properties;
using NTwain.Triplets;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]
[assembly: TargetFramework(".NETFramework,Version=v4.6.2", FrameworkDisplayName = ".NET Framework 4.6.2")]
[assembly: AssemblyCompany("Ben Olden-Cooligan")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyDescription("Fork of NTwain for NAPS2.Sdk")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+216c8c614d1514638cace41e5aa7e7ce48448d78")]
[assembly: AssemblyProduct("NTwain")]
[assembly: AssemblyTitle("NTwain")]
[assembly: AssemblyMetadata("RepositoryUrl", "https://github.com/cyanfish/ntwain")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AssemblyVersion("1.0.0.0")]
namespace NTwain
{
	/// <summary>
	/// Exposes capabilities of a data source as properties.
	/// </summary>
	public class Capabilities : ICapabilities
	{
		private IDataSource _source;

		private CapWrapper<XferMech> _audXferMech;

		private CapWrapper<CompressionType> _compression;

		private CapWrapper<PixelType> _pixelType;

		private CapWrapper<Unit> _imgUnits;

		private CapWrapper<XferMech> _imgXferMech;

		private CapWrapper<BoolType> _autoBright;

		private CapWrapper<TWFix32> _brightness;

		private CapWrapper<TWFix32> _contrast;

		private CapWrapper<byte> _custHalftone;

		private CapWrapper<TWFix32> _exposureTime;

		private CapWrapper<FilterType> _filter;

		private CapWrapper<TWFix32> _gamma;

		private CapWrapper<string> _halftones;

		private CapWrapper<TWFix32> _highlight;

		private CapWrapper<FileFormat> _fileFormat;

		private CapWrapper<BoolType> _lampState;

		private CapWrapper<LightSource> _lightSource;

		private CapWrapper<OrientationType> _orientation;

		private CapWrapper<TWFix32> _physicalWidth;

		private CapWrapper<TWFix32> _physicalHeight;

		private CapWrapper<TWFix32> _shadow;

		private CapWrapper<TWFrame> _frames;

		private CapWrapper<TWFix32> _nativeXRes;

		private CapWrapper<TWFix32> _nativeYRes;

		private CapWrapper<TWFix32> _xResolution;

		private CapWrapper<TWFix32> _yResolution;

		private CapWrapper<int> _maxFrames;

		private CapWrapper<BoolType> _tiles;

		private CapWrapper<BitOrder> _bitOrder;

		private CapWrapper<int> _ccittKFactor;

		private CapWrapper<LightPath> _lightPath;

		private CapWrapper<PixelFlavor> _pixelFlavor;

		private CapWrapper<PlanarChunky> _planarChunky;

		private CapWrapper<TWFix32> _rotation;

		private CapWrapper<SupportedSize> _supportSize;

		private CapWrapper<TWFix32> _threshold;

		private CapWrapper<TWFix32> _xscaling;

		private CapWrapper<TWFix32> _yscaling;

		private CapWrapper<BitOrder> _bitorderCodes;

		private CapWrapper<PixelFlavor> _pixelFlavorCodes;

		private CapWrapper<PixelType> _jpegPixelType;

		private CapWrapper<int> _timeFill;

		private CapWrapper<int> _bitDepth;

		private CapWrapper<BitDepthReduction> _bitDepthReduction;

		private CapWrapper<BoolType> _undefinedImgSize;

		private CapWrapper<uint> _imgDataSet;

		private CapWrapper<BoolType> _extImgInfo;

		private CapWrapper<TWFix32> _minHeight;

		private CapWrapper<TWFix32> _minWidth;

		private CapWrapper<BlankPage> _autoDiscBlankPg;

		private CapWrapper<FlipRotation> _flipRotation;

		private CapWrapper<BoolType> _barcodeDetectEnabled;

		private CapWrapper<BarcodeType> _barcodeType;

		private CapWrapper<uint> _barcodeMaxPriority;

		private CapWrapper<BarcodeType> _barcodeSearchPriority;

		private CapWrapper<BarcodeDirection> _barcodeSearchMode;

		private CapWrapper<uint> _barcodeMaxRetries;

		private CapWrapper<uint> _barcodeTimeout;

		private CapWrapper<int> _zoomFactor;

		private CapWrapper<BoolType> _patchcodeDetectEnabled;

		private CapWrapper<PatchCode> _patchcodeType;

		private CapWrapper<uint> _patchcodeMaxPriority;

		private CapWrapper<PatchCode> _patchcodeSearchPriority;

		private CapWrapper<BarcodeDirection> _patchcodeSearchMode;

		private CapWrapper<uint> _patchCodeMaxRetries;

		private CapWrapper<uint> _patchCodeTimeout;

		private CapWrapper<FlashedUsed> _flashUsed2;

		private CapWrapper<ImageFilter> _imgFilter;

		private CapWrapper<NoiseFilter> _noiseFilter;

		private CapWrapper<OverScan> _overscan;

		private CapWrapper<BoolType> _borderDetect;

		private CapWrapper<BoolType> _autoDeskew;

		private CapWrapper<BoolType> _autoRotate;

		private CapWrapper<JpegQuality> _jpegQuality;

		private CapWrapper<FeederType> _feederType;

		private CapWrapper<NTwain.Data.IccProfile> _iccProfile;

		private CapWrapper<AutoSize> _autoSize;

		private CapWrapper<BoolType> _cropUseFrame;

		private CapWrapper<BoolType> _lengthDetect;

		private CapWrapper<BoolType> _autoColor;

		private CapWrapper<PixelType> _autoColorNonPixel;

		private CapWrapper<BoolType> _colorMgmt;

		private CapWrapper<ImageMerge> _imgMerge;

		private CapWrapper<TWFix32> _mergeHeight;

		private CapWrapper<ExtendedImageInfo> _supportedExtInfo;

		private CapWrapper<FilmType> _filmType;

		private CapWrapper<Mirror> _mirror;

		private CapWrapper<JpegSubsampling> _jpegSubSampling;

		private CapWrapper<int> _xferCount;

		private CapWrapper<string> _author;

		private CapWrapper<string> _caption;

		private CapWrapper<BoolType> _feederEnabled;

		private CapWrapper<BoolType> _feederLoaded;

		private CapWrapper<string> _timedate;

		private CapWrapper<CapabilityId> _supportedCaps;

		private CapWrapper<CapabilityId> _extendedCaps;

		private CapWrapper<BoolType> _autoFeed;

		private CapWrapper<BoolType> _clearPage;

		private CapWrapper<BoolType> _feedPage;

		private CapWrapper<BoolType> _rewindPage;

		private CapWrapper<BoolType> _indicators;

		private CapWrapper<BoolType> _paperDetectable;

		private CapWrapper<BoolType> _uiControllable;

		private CapWrapper<BoolType> _devOnline;

		private CapWrapper<BoolType> _autoScan;

		private CapWrapper<BoolType> _thumbsEnabled;

		private CapWrapper<Duplex> _duplex;

		private CapWrapper<BoolType> _duplexEnabled;

		private CapWrapper<BoolType> _dsUIonly;

		private CapWrapper<BoolType> _dsData;

		private CapWrapper<uint> _endorser;

		private CapWrapper<JobControl> _jobControl;

		private CapWrapper<AlarmType> _alarms;

		private CapWrapper<int> _alarmVolume;

		private CapWrapper<int> _autoCapture;

		private CapWrapper<int> _timeBeforeCap;

		private CapWrapper<int> _timeBetweenCap;

		private CapWrapper<ClearBuffer> _clearBuff;

		private CapWrapper<uint> _maxBatchBuff;

		private CapWrapper<string> _devTimeDate;

		private CapWrapper<PowerSupply> _powerSup;

		private CapWrapper<BoolType> _camPreviewUI;

		private CapWrapper<NTwain.Data.DeviceEvent> _devEvent;

		private CapWrapper<string> _serialNo;

		private CapWrapper<Printer> _printer;

		private CapWrapper<BoolType> _printerEnabled;

		private CapWrapper<int> _printerIndex;

		private CapWrapper<PrinterMode> _printerMode;

		private CapWrapper<string> _printerString;

		private CapWrapper<string> _printerSuffix;

		private CapWrapper<Language> _language;

		private CapWrapper<FeederAlignment> _feedAlign;

		private CapWrapper<FeederOrder> _feedOrder;

		private CapWrapper<BoolType> _reacuireAllow;

		private CapWrapper<int> _battMinutes;

		private CapWrapper<int> _battPercent;

		private CapWrapper<CameraSide> _camSide;

		private CapWrapper<Segmented> _segmented;

		private CapWrapper<BoolType> _camEnabled;

		private CapWrapper<PixelType> _camOrder;

		private CapWrapper<BoolType> _micrEnabled;

		private CapWrapper<BoolType> _feederPrep;

		private CapWrapper<FeederPocket> _feedPocket;

		private CapWrapper<BoolType> _autoMedium;

		private CapWrapper<string> _custGuid;

		private CapWrapper<CapabilityId> _supportedCapsUnique;

		private CapWrapper<uint> _supportedDat;

		private CapWrapper<DoubleFeedDetection> _dblFeedDetect;

		private CapWrapper<TWFix32> _dblFeedLength;

		private CapWrapper<DoubleFeedDetectionSensitivity> _dblFeedSensitivity;

		private CapWrapper<DoubleFeedDetectionResponse> _dblFeedResp;

		private CapWrapper<PaperHandling> _paperHandling;

		private CapWrapper<IndicatorsMode> _indicatorMode;

		private CapWrapper<TWFix32> _printVOffset;

		private CapWrapper<int> _powerSaveTime;

		private CapWrapper<int> _printCharRot;

		private CapWrapper<PrinterFontStyle> _printFontStyle;

		private CapWrapper<string> _printerIdxLeadChar;

		private CapWrapper<uint> _printIdxMax;

		private CapWrapper<int> _printNumDigit;

		private CapWrapper<int> _printIdxStep;

		private CapWrapper<PrinterIndexTrigger> _printIdxTrig;

		private CapWrapper<string> _printPreview;

		/// <summary>
		/// Gets the property to work with audio <see cref="T:NTwain.Data.XferMech" /> for the current source.
		/// </summary>
		/// <value>
		/// The audio xfer mech.
		/// </value>
		public ICapWrapper<XferMech> ACapXferMech => _audXferMech ?? (_audXferMech = new CapWrapper<XferMech>(_source, CapabilityId.ACapXferMech, ValueExtensions.ConvertToEnum<XferMech>, (XferMech value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.CompressionType" /> for the current source.
		/// </summary>
		/// <value>
		/// The image compression.
		/// </value>
		public ICapWrapper<CompressionType> ICapCompression => _compression ?? (_compression = new CapWrapper<CompressionType>(_source, CapabilityId.ICapCompression, ValueExtensions.ConvertToEnum<CompressionType>, (CompressionType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.PixelType" /> for the current source.
		/// </summary>
		/// <value>
		/// The image pixel type.
		/// </value>
		public ICapWrapper<PixelType> ICapPixelType => _pixelType ?? (_pixelType = new CapWrapper<PixelType>(_source, CapabilityId.ICapPixelType, ValueExtensions.ConvertToEnum<PixelType>, (PixelType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.Unit" /> for the current source.
		/// </summary>
		/// <value>
		/// The image unit of measure.
		/// </value>
		public ICapWrapper<Unit> ICapUnits => _imgUnits ?? (_imgUnits = new CapWrapper<Unit>(_source, CapabilityId.ICapUnits, ValueExtensions.ConvertToEnum<Unit>, (Unit value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.XferMech" /> for the current source.
		/// </summary>
		/// <value>
		/// The image xfer mech.
		/// </value>
		public ICapWrapper<XferMech> ICapXferMech => _imgXferMech ?? (_imgXferMech = new CapWrapper<XferMech>(_source, CapabilityId.ICapXferMech, ValueExtensions.ConvertToEnum<XferMech>, (XferMech value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image auto brightness flag for the current source.
		/// </summary>
		/// <value>
		/// The image auto brightness flag.
		/// </value>
		public ICapWrapper<BoolType> ICapAutoBright => _autoBright ?? (_autoBright = new CapWrapper<BoolType>(_source, CapabilityId.ICapAutoBright, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with image brightness for the current source.
		/// </summary>
		/// <value>
		/// The image brightness.
		/// </value>
		public ICapWrapper<TWFix32> ICapBrightness => _brightness ?? (_brightness = new CapWrapper<TWFix32>(_source, CapabilityId.ICapBrightness, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with image contrast for the current source.
		/// </summary>
		/// <value>
		/// The image contrast.
		/// </value>
		public ICapWrapper<TWFix32> ICapContrast => _contrast ?? (_contrast = new CapWrapper<TWFix32>(_source, CapabilityId.ICapContrast, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with image square-cell halftone for the current source.
		/// </summary>
		/// <value>
		/// The image square-cell halftone.
		/// </value>
		public ICapWrapper<byte> ICapCustHalftone => _custHalftone ?? (_custHalftone = new CapWrapper<byte>(_source, CapabilityId.ICapCustHalftone, ValueExtensions.ConvertToEnum<byte>, (byte value) => new TWOneValue
		{
			Item = value,
			ItemType = ItemType.UInt8
		}));

		/// <summary>
		/// Gets the property to work with image exposure time (in seconds) for the current source.
		/// </summary>
		/// <value>
		/// The image exposure time.
		/// </value>
		public ICapWrapper<TWFix32> ICapExposureTime => _exposureTime ?? (_exposureTime = new CapWrapper<TWFix32>(_source, CapabilityId.ICapExposureTime, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with image color filter for the current source.
		/// </summary>
		/// <value>
		/// The image color filter type.
		/// </value>
		public ICapWrapper<FilterType> ICapFilter => _filter ?? (_filter = new CapWrapper<FilterType>(_source, CapabilityId.ICapFilter, ValueExtensions.ConvertToEnum<FilterType>, (FilterType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image gamma value for the current source.
		/// </summary>
		/// <value>
		/// The image gamma.
		/// </value>
		public ICapWrapper<TWFix32> ICapGamma => _gamma ?? (_gamma = new CapWrapper<TWFix32>(_source, CapabilityId.ICapGamma, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with image halftone patterns for the current source.
		/// </summary>
		/// <value>
		/// The image halftone patterns.
		/// </value>
		public ICapWrapper<string> ICapHalftones => _halftones ?? (_halftones = new CapWrapper<string>(_source, CapabilityId.ICapHalftones, ValueExtensions.ConvertToString, readOnly: false));

		/// <summary>
		/// Gets the property to work with image highlight value for the current source.
		/// </summary>
		/// <value>
		/// The image highlight.
		/// </value>
		public ICapWrapper<TWFix32> ICapHighlight => _highlight ?? (_highlight = new CapWrapper<TWFix32>(_source, CapabilityId.ICapHighlight, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.FileFormat" /> for the current source.
		/// </summary>
		/// <value>
		/// The image file format.
		/// </value>
		public ICapWrapper<FileFormat> ICapImageFileFormat => _fileFormat ?? (_fileFormat = new CapWrapper<FileFormat>(_source, CapabilityId.ICapImageFileFormat, ValueExtensions.ConvertToEnum<FileFormat>, (FileFormat value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image lamp state flag for the current source.
		/// </summary>
		/// <value>
		/// The image lamp state flag.
		/// </value>
		public ICapWrapper<BoolType> ICapLampState => _lampState ?? (_lampState = new CapWrapper<BoolType>(_source, CapabilityId.ICapLampState, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with image light source for the current source.
		/// </summary>
		/// <value>
		/// The image light source.
		/// </value>
		public ICapWrapper<LightSource> ICapLightSource => _lightSource ?? (_lightSource = new CapWrapper<LightSource>(_source, CapabilityId.ICapLightSource, ValueExtensions.ConvertToEnum<LightSource>, (LightSource value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image orientation for the current source.
		/// </summary>
		/// <value>
		/// The image orientation.
		/// </value>
		public ICapWrapper<OrientationType> ICapOrientation => _orientation ?? (_orientation = new CapWrapper<OrientationType>(_source, CapabilityId.ICapOrientation, ValueExtensions.ConvertToEnum<OrientationType>, (OrientationType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image physical width for the current source.
		/// </summary>
		/// <value>
		/// The image physical width.
		/// </value>
		public IReadOnlyCapWrapper<TWFix32> ICapPhysicalWidth => _physicalWidth ?? (_physicalWidth = new CapWrapper<TWFix32>(_source, CapabilityId.ICapPhysicalWidth, ValueExtensions.ConvertToFix32, readOnly: true));

		/// <summary>
		/// Gets the property to work with image physical height for the current source.
		/// </summary>
		/// <value>
		/// The image physical height.
		/// </value>
		public IReadOnlyCapWrapper<TWFix32> ICapPhysicalHeight => _physicalHeight ?? (_physicalHeight = new CapWrapper<TWFix32>(_source, CapabilityId.ICapPhysicalHeight, ValueExtensions.ConvertToFix32, readOnly: true));

		/// <summary>
		/// Gets the property to work with image shadow value for the current source.
		/// </summary>
		/// <value>
		/// The image shadow.
		/// </value>
		public ICapWrapper<TWFix32> ICapShadow => _shadow ?? (_shadow = new CapWrapper<TWFix32>(_source, CapabilityId.ICapShadow, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with the list of frames the source will acquire on each page.
		/// </summary>
		/// <value>
		/// The capture frames.
		/// </value>
		public ICapWrapper<TWFrame> ICapFrames => _frames ?? (_frames = new CapWrapper<TWFrame>(_source, CapabilityId.ICapFrames, ValueExtensions.ConvertToFrame, (TWFrame value) =>
		{
			using TWCapability capability = new TWCapability(CapabilityId.ICapFrames, value);
			return _source.DGControl.Capability.Set(capability);
		}));

		/// <summary>
		/// Gets the property to work with image's native x-axis resolution for the current source.
		/// </summary>
		/// <value>
		/// The image's native x-axis resolution.
		/// </value>
		public IReadOnlyCapWrapper<TWFix32> ICapXNativeResolution => _nativeXRes ?? (_nativeXRes = new CapWrapper<TWFix32>(_source, CapabilityId.ICapXNativeResolution, ValueExtensions.ConvertToFix32, readOnly: true));

		/// <summary>
		/// Gets the property to work with image's native y-axis resolution for the current source.
		/// </summary>
		/// <value>
		/// The image's native y-axis resolution.
		/// </value>
		public IReadOnlyCapWrapper<TWFix32> ICapYNativeResolution => _nativeYRes ?? (_nativeYRes = new CapWrapper<TWFix32>(_source, CapabilityId.ICapYNativeResolution, ValueExtensions.ConvertToFix32, readOnly: true));

		/// <summary>
		/// Gets the property to work with image x-axis resolution for the current source.
		/// </summary>
		/// <value>
		/// The image x-axis resolution.
		/// </value>
		public ICapWrapper<TWFix32> ICapXResolution => _xResolution ?? (_xResolution = new CapWrapper<TWFix32>(_source, CapabilityId.ICapXResolution, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with image y-axis resolution for the current source.
		/// </summary>
		/// <value>
		/// The image y-axis resolution.
		/// </value>
		public ICapWrapper<TWFix32> ICapYResolution => _yResolution ?? (_yResolution = new CapWrapper<TWFix32>(_source, CapabilityId.ICapYResolution, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with image max frames for the current source.
		/// </summary>
		/// <value>
		/// The image max frames.
		/// </value>
		public ICapWrapper<int> ICapMaxFrames => _maxFrames ?? (_maxFrames = new CapWrapper<int>(_source, CapabilityId.ICapMaxFrames, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image tiles flag for the current source.
		/// </summary>
		/// <value>
		/// The image tiles flag.
		/// </value>
		public ICapWrapper<BoolType> ICapTiles => _tiles ?? (_tiles = new CapWrapper<BoolType>(_source, CapabilityId.ICapTiles, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.BitOrder" /> for the current source.
		/// </summary>
		/// <value>
		/// The image bit order.
		/// </value>
		public ICapWrapper<BitOrder> ICapBitOrder => _bitOrder ?? (_bitOrder = new CapWrapper<BitOrder>(_source, CapabilityId.ICapBitOrder, ValueExtensions.ConvertToEnum<BitOrder>, (BitOrder value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image CCITT K factor for the current source.
		/// </summary>
		/// <value>
		/// The image CCITT K factor.
		/// </value>
		public ICapWrapper<int> ICapCCITTKFactor => _ccittKFactor ?? (_ccittKFactor = new CapWrapper<int>(_source, CapabilityId.ICapCCITTKFactor, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image light path for the current source.
		/// </summary>
		/// <value>
		/// The image light path.
		/// </value>
		public ICapWrapper<LightPath> ICapLightPath => _lightPath ?? (_lightPath = new CapWrapper<LightPath>(_source, CapabilityId.ICapLightPath, ValueExtensions.ConvertToEnum<LightPath>, (LightPath value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image pixel flavor for the current source.
		/// </summary>
		/// <value>
		/// The image pixel flavor.
		/// </value>
		public ICapWrapper<PixelFlavor> ICapPixelFlavor => _pixelFlavor ?? (_pixelFlavor = new CapWrapper<PixelFlavor>(_source, CapabilityId.ICapPixelFlavor, ValueExtensions.ConvertToEnum<PixelFlavor>, (PixelFlavor value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image color format for the current source.
		/// </summary>
		/// <value>
		/// The image color format.
		/// </value>
		public ICapWrapper<PlanarChunky> ICapPlanarChunky => _planarChunky ?? (_planarChunky = new CapWrapper<PlanarChunky>(_source, CapabilityId.ICapPlanarChunky, ValueExtensions.ConvertToEnum<PlanarChunky>, (PlanarChunky value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image rotation for the current source.
		/// </summary>
		/// <value>
		/// The image rotation.
		/// </value>
		public ICapWrapper<TWFix32> ICapRotation => _rotation ?? (_rotation = new CapWrapper<TWFix32>(_source, CapabilityId.ICapRotation, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.SupportedSize" /> for the current source.
		/// </summary>
		/// <value>
		/// The image supported size.
		/// </value>
		public ICapWrapper<SupportedSize> ICapSupportedSizes => _supportSize ?? (_supportSize = new CapWrapper<SupportedSize>(_source, CapabilityId.ICapSupportedSizes, ValueExtensions.ConvertToEnum<SupportedSize>, (SupportedSize value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image threshold for the current source.
		/// </summary>
		/// <value>
		/// The image threshold.
		/// </value>
		public ICapWrapper<TWFix32> ICapThreshold => _threshold ?? (_threshold = new CapWrapper<TWFix32>(_source, CapabilityId.ICapThreshold, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with image x-axis scaling for the current source.
		/// </summary>
		/// <value>
		/// The image x-axis scaling.
		/// </value>
		public ICapWrapper<TWFix32> ICapXScaling => _xscaling ?? (_xscaling = new CapWrapper<TWFix32>(_source, CapabilityId.ICapXScaling, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with image y-axis scaling for the current source.
		/// </summary>
		/// <value>
		/// The image y-axis scaling.
		/// </value>
		public ICapWrapper<TWFix32> ICapYScaling => _yscaling ?? (_yscaling = new CapWrapper<TWFix32>(_source, CapabilityId.ICapYScaling, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with image CCITT compression <see cref="T:NTwain.Data.BitOrder" /> for the current source.
		/// </summary>
		/// <value>
		/// The image bit order for CCITT compression.
		/// </value>
		public ICapWrapper<BitOrder> ICapBitOrderCodes => _bitorderCodes ?? (_bitorderCodes = new CapWrapper<BitOrder>(_source, CapabilityId.ICapBitOrderCodes, ValueExtensions.ConvertToEnum<BitOrder>, (BitOrder value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image CCITT compression <see cref="T:NTwain.Data.PixelFlavor" /> for the current source.
		/// </summary>
		/// <value>
		/// The image pixel flavor for CCITT compression.
		/// </value>
		public ICapWrapper<PixelFlavor> ICapPixelFlavorCodes => _pixelFlavorCodes ?? (_pixelFlavorCodes = new CapWrapper<PixelFlavor>(_source, CapabilityId.ICapPixelFlavorCodes, ValueExtensions.ConvertToEnum<PixelFlavor>, (PixelFlavor value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image jpeg compression <see cref="T:NTwain.Data.PixelType" /> for the current source.
		/// </summary>
		/// <value>
		/// The image pixel type for jpeg compression.
		/// </value>
		public ICapWrapper<PixelType> ICapJpegPixelType => _jpegPixelType ?? (_jpegPixelType = new CapWrapper<PixelType>(_source, CapabilityId.ICapJpegPixelType, ValueExtensions.ConvertToEnum<PixelType>, (PixelType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image CCITT time fill for the current source.
		/// </summary>
		/// <value>
		/// The image CCITT time fill.
		/// </value>
		public ICapWrapper<int> ICapTimeFill => _timeFill ?? (_timeFill = new CapWrapper<int>(_source, CapabilityId.ICapTimeFill, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image bit depth for the current source.
		/// </summary>
		/// <value>
		/// The image bit depth.
		/// </value>
		public ICapWrapper<int> ICapBitDepth => _bitDepth ?? (_bitDepth = new CapWrapper<int>(_source, CapabilityId.ICapBitDepth, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image bit depth reduction method for the current source.
		/// </summary>
		/// <value>
		/// The image bit depth reduction method.
		/// </value>
		public ICapWrapper<BitDepthReduction> ICapBitDepthReduction => _bitDepthReduction ?? (_bitDepthReduction = new CapWrapper<BitDepthReduction>(_source, CapabilityId.ICapBitDepthReduction, ValueExtensions.ConvertToEnum<BitDepthReduction>, (BitDepthReduction value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image undefined size flag for the current source.
		/// </summary>
		/// <value>
		/// The image undefined size flag.
		/// </value>
		public ICapWrapper<BoolType> ICapUndefinedImageSize => _undefinedImgSize ?? (_undefinedImgSize = new CapWrapper<BoolType>(_source, CapabilityId.ICapUndefinedImageSize, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets or sets the image indices that will be delivered during the standard image transfer done in
		/// States 6 and 7.
		/// </summary>
		/// <value>
		/// The image indicies.
		/// </value>
		public ICapWrapper<uint> ICapImageDataSet => _imgDataSet ?? (_imgDataSet = new CapWrapper<uint>(_source, CapabilityId.ICapImageDataSet, ValueExtensions.ConvertToEnum<uint>, (uint value) => new TWOneValue
		{
			Item = value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with ext image info flag for the current source.
		/// </summary>
		/// <value>
		/// The ext image info flag.
		/// </value>
		public ICapWrapper<BoolType> ICapExtImageInfo => _extImgInfo ?? (_extImgInfo = new CapWrapper<BoolType>(_source, CapabilityId.ICapExtImageInfo, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with image minimum height for the current source.
		/// </summary>
		/// <value>
		/// The image minimumm height.
		/// </value>
		public IReadOnlyCapWrapper<TWFix32> ICapMinimumHeight => _minHeight ?? (_minHeight = new CapWrapper<TWFix32>(_source, CapabilityId.ICapMinimumHeight, ValueExtensions.ConvertToFix32, readOnly: true));

		/// <summary>
		/// Gets the property to work with image minimum width for the current source.
		/// </summary>
		/// <value>
		/// The image minimumm width.
		/// </value>
		public IReadOnlyCapWrapper<TWFix32> ICapMinimumWidth => _minWidth ?? (_minWidth = new CapWrapper<TWFix32>(_source, CapabilityId.ICapMinimumWidth, ValueExtensions.ConvertToFix32, readOnly: true));

		/// <summary>
		/// Gets the property to work with image blank page behavior for the current source.
		/// </summary>
		/// <value>
		/// The image blank page behavior.
		/// </value>
		public ICapWrapper<BlankPage> ICapAutoDiscardBlankPages => _autoDiscBlankPg ?? (_autoDiscBlankPg = new CapWrapper<BlankPage>(_source, CapabilityId.ICapAutoDiscardBlankPages, ValueExtensions.ConvertToEnum<BlankPage>, (BlankPage value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Int32
		}));

		/// <summary>
		/// Gets the property to work with image flip-rotation behavior for the current source.
		/// </summary>
		/// <value>
		/// The image flip-rotation behavior.
		/// </value>
		public ICapWrapper<FlipRotation> ICapFlipRotation => _flipRotation ?? (_flipRotation = new CapWrapper<FlipRotation>(_source, CapabilityId.ICapFlipRotation, ValueExtensions.ConvertToEnum<FlipRotation>, (FlipRotation value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Int32
		}));

		/// <summary>
		/// Gets the property to work with image barcode detection flag for the current source.
		/// </summary>
		/// <value>
		/// The image barcode detection flag.
		/// </value>
		public ICapWrapper<BoolType> ICapBarcodeDetectionEnabled => _barcodeDetectEnabled ?? (_barcodeDetectEnabled = new CapWrapper<BoolType>(_source, CapabilityId.ICapBarcodeDetectionEnabled, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with image barcode types for the current source.
		/// </summary>
		/// <value>
		/// The image barcode types.
		/// </value>
		public IReadOnlyCapWrapper<BarcodeType> ICapSupportedBarcodeTypes => _barcodeType ?? (_barcodeType = new CapWrapper<BarcodeType>(_source, CapabilityId.ICapSupportedBarcodeTypes, ValueExtensions.ConvertToEnum<BarcodeType>, readOnly: true));

		/// <summary>
		/// Gets the property to work with image barcode max search priorities for the current source.
		/// </summary>
		/// <value>
		/// The image barcode max search priorities.
		/// </value>
		public ICapWrapper<uint> ICapBarcodeMaxSearchPriorities => _barcodeMaxPriority ?? (_barcodeMaxPriority = new CapWrapper<uint>(_source, CapabilityId.ICapBarcodeMaxSearchPriorities, ValueExtensions.ConvertToEnum<uint>, (uint value) => new TWOneValue
		{
			Item = value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with image barcode search priority for the current source.
		/// </summary>
		/// <value>
		/// The image barcode search priority.
		/// </value>
		public ICapWrapper<BarcodeType> ICapBarcodeSearchPriorities => _barcodeSearchPriority ?? (_barcodeSearchPriority = new CapWrapper<BarcodeType>(_source, CapabilityId.ICapBarcodeSearchPriorities, ValueExtensions.ConvertToEnum<BarcodeType>, (BarcodeType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image barcode search direction for the current source.
		/// </summary>
		/// <value>
		/// The image barcode search direction.
		/// </value>
		public ICapWrapper<BarcodeDirection> ICapBarcodeSearchMode => _barcodeSearchMode ?? (_barcodeSearchMode = new CapWrapper<BarcodeDirection>(_source, CapabilityId.ICapBarcodeSearchMode, ValueExtensions.ConvertToEnum<BarcodeDirection>, (BarcodeDirection value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image barcode max search retries for the current source.
		/// </summary>
		/// <value>
		/// The image barcode max search retries.
		/// </value>
		public ICapWrapper<uint> ICapBarcodeMaxRetries => _barcodeMaxRetries ?? (_barcodeMaxRetries = new CapWrapper<uint>(_source, CapabilityId.ICapBarcodeMaxRetries, ValueExtensions.ConvertToEnum<uint>, (uint value) => new TWOneValue
		{
			Item = value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with image barcode max search timeout for the current source.
		/// </summary>
		/// <value>
		/// The image barcode max search timeout.
		/// </value>
		public ICapWrapper<uint> ICapBarcodeTimeout => _barcodeTimeout ?? (_barcodeTimeout = new CapWrapper<uint>(_source, CapabilityId.ICapBarcodeTimeout, ValueExtensions.ConvertToEnum<uint>, (uint value) => new TWOneValue
		{
			Item = value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with image zoom factor for the current source.
		/// </summary>
		/// <value>
		/// The image zoom factor.
		/// </value>
		public ICapWrapper<int> ICapZoomFactor => _zoomFactor ?? (_zoomFactor = new CapWrapper<int>(_source, CapabilityId.ICapZoomFactor, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image patch code detection flag for the current source.
		/// </summary>
		/// <value>
		/// The image patch code detection flag.
		/// </value>
		public ICapWrapper<BoolType> ICapPatchCodeDetectionEnabled => _patchcodeDetectEnabled ?? (_patchcodeDetectEnabled = new CapWrapper<BoolType>(_source, CapabilityId.ICapPatchCodeDetectionEnabled, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with image patch code types for the current source.
		/// </summary>
		/// <value>
		/// The image patch code types.
		/// </value>
		public IReadOnlyCapWrapper<PatchCode> ICapSupportedPatchCodeTypes => _patchcodeType ?? (_patchcodeType = new CapWrapper<PatchCode>(_source, CapabilityId.ICapSupportedPatchCodeTypes, ValueExtensions.ConvertToEnum<PatchCode>, readOnly: true));

		/// <summary>
		/// Gets the property to work with image patch code max search priorities for the current source.
		/// </summary>
		/// <value>
		/// The image patch code max search priorities.
		/// </value>
		public ICapWrapper<uint> ICapPatchCodeMaxSearchPriorities => _patchcodeMaxPriority ?? (_patchcodeMaxPriority = new CapWrapper<uint>(_source, CapabilityId.ICapPatchCodeMaxSearchPriorities, ValueExtensions.ConvertToEnum<uint>, (uint value) => new TWOneValue
		{
			Item = value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with image patch code search priority for the current source.
		/// </summary>
		/// <value>
		/// The image patch code search priority.
		/// </value>
		public ICapWrapper<PatchCode> ICapPatchCodeSearchPriorities => _patchcodeSearchPriority ?? (_patchcodeSearchPriority = new CapWrapper<PatchCode>(_source, CapabilityId.ICapPatchCodeSearchPriorities, ValueExtensions.ConvertToEnum<PatchCode>, (PatchCode value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image patch code search direction for the current source.
		/// </summary>
		/// <value>
		/// The image patch code search direction.
		/// </value>
		public ICapWrapper<BarcodeDirection> ICapPatchCodeSearchMode => _patchcodeSearchMode ?? (_patchcodeSearchMode = new CapWrapper<BarcodeDirection>(_source, CapabilityId.ICapPatchCodeSearchMode, ValueExtensions.ConvertToEnum<BarcodeDirection>, (BarcodeDirection value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image patch code max search retries for the current source.
		/// </summary>
		/// <value>
		/// The image patch code max search retries.
		/// </value>
		public ICapWrapper<uint> ICapPatchCodeMaxRetries => _patchCodeMaxRetries ?? (_patchCodeMaxRetries = new CapWrapper<uint>(_source, CapabilityId.ICapPatchCodeMaxRetries, ValueExtensions.ConvertToEnum<uint>, (uint value) => new TWOneValue
		{
			Item = value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with image patch code max search timeout for the current source.
		/// </summary>
		/// <value>
		/// The image patch code max search timeout.
		/// </value>
		public ICapWrapper<uint> ICapPatchCodeTimeout => _patchCodeTimeout ?? (_patchCodeTimeout = new CapWrapper<uint>(_source, CapabilityId.ICapPatchCodeTimeout, ValueExtensions.ConvertToEnum<uint>, (uint value) => new TWOneValue
		{
			Item = value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with flash option for the current source.
		/// </summary>
		/// <value>
		/// The flash option.
		/// </value>
		public ICapWrapper<FlashedUsed> ICapFlashUsed2 => _flashUsed2 ?? (_flashUsed2 = new CapWrapper<FlashedUsed>(_source, CapabilityId.ICapFlashUsed2, ValueExtensions.ConvertToEnum<FlashedUsed>, (FlashedUsed value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image enhancement filter for the current source.
		/// </summary>
		/// <value>
		/// The image enhancement filter.
		/// </value>
		public ICapWrapper<ImageFilter> ICapImageFilter => _imgFilter ?? (_imgFilter = new CapWrapper<ImageFilter>(_source, CapabilityId.ICapImageFilter, ValueExtensions.ConvertToEnum<ImageFilter>, (ImageFilter value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image noise filter for the current source.
		/// </summary>
		/// <value>
		/// The image noise filter.
		/// </value>
		public ICapWrapper<NoiseFilter> ICapNoiseFilter => _noiseFilter ?? (_noiseFilter = new CapWrapper<NoiseFilter>(_source, CapabilityId.ICapNoiseFilter, ValueExtensions.ConvertToEnum<NoiseFilter>, (NoiseFilter value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image overscan option for the current source.
		/// </summary>
		/// <value>
		/// The image overscan option.
		/// </value>
		public ICapWrapper<OverScan> ICapOverScan => _overscan ?? (_overscan = new CapWrapper<OverScan>(_source, CapabilityId.ICapOverScan, ValueExtensions.ConvertToEnum<OverScan>, (OverScan value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with auto border detection flag for the current source.
		/// </summary>
		/// <value>
		/// The auto border detection flag.
		/// </value>
		public ICapWrapper<BoolType> ICapAutomaticBorderDetection => _borderDetect ?? (_borderDetect = new CapWrapper<BoolType>(_source, CapabilityId.ICapAutomaticBorderDetection, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) =>
		{
			ReturnCode returnCode = ReturnCode.Failure;
			TWOneValue value2 = new TWOneValue
			{
				Item = (uint)value,
				ItemType = ItemType.Bool
			};
			returnCode = ICapUndefinedImageSize.SetValue(value);
			using TWCapability capability = new TWCapability(CapabilityId.ICapAutomaticBorderDetection, value2);
			return _source.DGControl.Capability.Set(capability);
		}));

		/// <summary>
		/// Gets the property to work with image auto deskew flag for the current source.
		/// </summary>
		/// <value>
		/// The image auto deskew flag.
		/// </value>
		public ICapWrapper<BoolType> ICapAutomaticDeskew => _autoDeskew ?? (_autoDeskew = new CapWrapper<BoolType>(_source, CapabilityId.ICapAutomaticDeskew, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with image auto rotate flag for the current source.
		/// </summary>
		/// <value>
		/// The image auto rotate flag.
		/// </value>
		public ICapWrapper<BoolType> ICapAutomaticRotate => _autoRotate ?? (_autoRotate = new CapWrapper<BoolType>(_source, CapabilityId.ICapAutomaticRotate, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with image jpeg quality for the current source.
		/// </summary>
		/// <value>
		/// The image jpeg quality.
		/// </value>
		public ICapWrapper<JpegQuality> ICapJpegQuality => _jpegQuality ?? (_jpegQuality = new CapWrapper<JpegQuality>(_source, CapabilityId.ICapJpegQuality, ValueExtensions.ConvertToEnum<JpegQuality>, (JpegQuality value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Int16
		}));

		/// <summary>
		/// Gets the property to work with feeder type for the current source.
		/// </summary>
		/// <value>
		/// The feeder type.
		/// </value>
		public ICapWrapper<FeederType> ICapFeederType => _feederType ?? (_feederType = new CapWrapper<FeederType>(_source, CapabilityId.ICapFeederType, ValueExtensions.ConvertToEnum<FeederType>, (FeederType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image icc profile for the current source.
		/// </summary>
		/// <value>
		/// The image icc profile.
		/// </value>
		public ICapWrapper<NTwain.Data.IccProfile> ICapICCProfile => _iccProfile ?? (_iccProfile = new CapWrapper<NTwain.Data.IccProfile>(_source, CapabilityId.ICapICCProfile, ValueExtensions.ConvertToEnum<NTwain.Data.IccProfile>, (NTwain.Data.IccProfile value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image auto size option for the current source.
		/// </summary>
		/// <value>
		/// The image auto size option.
		/// </value>
		public ICapWrapper<AutoSize> ICapAutoSize => _autoSize ?? (_autoSize = new CapWrapper<AutoSize>(_source, CapabilityId.ICapAutoSize, ValueExtensions.ConvertToEnum<AutoSize>, (AutoSize value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image auto crop flag for the current source.
		/// </summary>
		/// <value>
		/// The image auto crop flag.
		/// </value>
		public IReadOnlyCapWrapper<BoolType> ICapAutomaticCropUsesFrame => _cropUseFrame ?? (_cropUseFrame = new CapWrapper<BoolType>(_source, CapabilityId.ICapAutomaticCropUsesFrame, ValueExtensions.ConvertToEnum<BoolType>, readOnly: true));

		/// <summary>
		/// Gets the property to work with image auto length detection flag for the current source.
		/// </summary>
		/// <value>
		/// The image auto length detection flag.
		/// </value>
		public ICapWrapper<BoolType> ICapAutomaticLengthDetection => _lengthDetect ?? (_lengthDetect = new CapWrapper<BoolType>(_source, CapabilityId.ICapAutomaticLengthDetection, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with image auto color detection flag for the current source.
		/// </summary>
		/// <value>
		/// The image auto color detection flag.
		/// </value>
		public ICapWrapper<BoolType> ICapAutomaticColorEnabled => _autoColor ?? (_autoColor = new CapWrapper<BoolType>(_source, CapabilityId.ICapAutomaticColorEnabled, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with image auto non-color pixel type for the current source.
		/// </summary>
		/// <value>
		/// The image auto non-color pixel type.
		/// </value>
		public ICapWrapper<PixelType> ICapAutomaticColorNonColorPixelType => _autoColorNonPixel ?? (_autoColorNonPixel = new CapWrapper<PixelType>(_source, CapabilityId.ICapAutomaticColorNonColorPixelType, ValueExtensions.ConvertToEnum<PixelType>, (PixelType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image color management flag for the current source.
		/// </summary>
		/// <value>
		/// The image color management flag.
		/// </value>
		public ICapWrapper<BoolType> ICapColorManagementEnabled => _colorMgmt ?? (_colorMgmt = new CapWrapper<BoolType>(_source, CapabilityId.ICapColorManagementEnabled, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with image merge option for the current source.
		/// </summary>
		/// <value>
		/// The image merge option.
		/// </value>
		public ICapWrapper<ImageMerge> ICapImageMerge => _imgMerge ?? (_imgMerge = new CapWrapper<ImageMerge>(_source, CapabilityId.ICapImageMerge, ValueExtensions.ConvertToEnum<ImageMerge>, (ImageMerge value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image merge height threshold for the current source.
		/// </summary>
		/// <value>
		/// The image merge height threshold.
		/// </value>
		public ICapWrapper<TWFix32> ICapImageMergeHeightThreshold => _mergeHeight ?? (_mergeHeight = new CapWrapper<TWFix32>(_source, CapabilityId.ICapImageMergeHeightThreshold, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to get supported ext image info for the current source.
		/// </summary>
		/// <value>
		/// The supported ext image info.
		/// </value>
		public IReadOnlyCapWrapper<ExtendedImageInfo> ICapSupportedExtImageInfo => _supportedExtInfo ?? (_supportedExtInfo = new CapWrapper<ExtendedImageInfo>(_source, CapabilityId.ICapSupportedExtImageInfo, ValueExtensions.ConvertToEnum<ExtendedImageInfo>, readOnly: true));

		/// <summary>
		/// Gets the property to work with image film type for the current source.
		/// </summary>
		/// <value>
		/// The image film type.
		/// </value>
		public ICapWrapper<FilmType> ICapFilmType => _filmType ?? (_filmType = new CapWrapper<FilmType>(_source, CapabilityId.ICapFilmType, ValueExtensions.ConvertToEnum<FilmType>, (FilmType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image mirror option for the current source.
		/// </summary>
		/// <value>
		/// The image mirror option.
		/// </value>
		public ICapWrapper<Mirror> ICapMirror => _mirror ?? (_mirror = new CapWrapper<Mirror>(_source, CapabilityId.ICapMirror, ValueExtensions.ConvertToEnum<Mirror>, (Mirror value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with image jpeg sub sampling for the current source.
		/// </summary>
		/// <value>
		/// The image jpeg sub sampling.
		/// </value>
		public ICapWrapper<JpegSubsampling> ICapJpegSubsampling => _jpegSubSampling ?? (_jpegSubSampling = new CapWrapper<JpegSubsampling>(_source, CapabilityId.ICapJpegSubsampling, ValueExtensions.ConvertToEnum<JpegSubsampling>, (JpegSubsampling value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with xfer count for the current source.
		/// </summary>
		/// <value>
		/// The xfer count.
		/// </value>
		public ICapWrapper<int> CapXferCount => _xferCount ?? (_xferCount = new CapWrapper<int>(_source, CapabilityId.CapXferCount, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = ((value > 0) ? ((uint)value) : uint.MaxValue),
			ItemType = ItemType.Int16
		}));

		/// <summary>
		/// Gets the property to work with the name or other identifying information about the 
		/// Author of the image. It may include a copyright string.
		/// </summary>
		/// <value>
		/// The author string.
		/// </value>
		public ICapWrapper<string> CapAuthor => _author ?? (_author = new CapWrapper<string>(_source, CapabilityId.CapAuthor, ValueExtensions.ConvertToString, (string value) =>
		{
			using TWCapability capability = new TWCapability(CapabilityId.CapAuthor, value, ItemType.String128);
			return _source.DGControl.Capability.Set(capability);
		}));

		/// <summary>
		/// Gets the property to work with the general note about the acquired image.
		/// </summary>
		/// <value>
		/// The general note string.
		/// </value>
		public ICapWrapper<string> CapCaption => _caption ?? (_caption = new CapWrapper<string>(_source, CapabilityId.CapCaption, ValueExtensions.ConvertToString, (string value) =>
		{
			using TWCapability capability = new TWCapability(CapabilityId.CapCaption, value, ItemType.String255);
			return _source.DGControl.Capability.Set(capability);
		}));

		/// <summary>
		/// Gets the property to work with feeder enabled flag for the current source.
		/// </summary>
		/// <value>
		/// The feeder enabled flag.
		/// </value>
		public ICapWrapper<BoolType> CapFeederEnabled => _feederEnabled ?? (_feederEnabled = new CapWrapper<BoolType>(_source, CapabilityId.CapFeederEnabled, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) =>
		{
			ReturnCode result = ReturnCode.Failure;
			TWOneValue value2 = new TWOneValue
			{
				Item = (uint)value,
				ItemType = ItemType.Bool
			};
			using (TWCapability capability = new TWCapability(CapabilityId.CapFeederEnabled, value2))
			{
				result = _source.DGControl.Capability.Set(capability);
			}
			if (CapAutoScan.CanSet)
			{
				result = CapAutoScan.SetValue(value);
			}
			else if (CapAutoFeed.CanSet)
			{
				result = CapAutoFeed.SetValue(value);
			}
			return result;
		}));

		/// <summary>
		/// Gets the property to work with feeder loaded flag for the current source.
		/// </summary>
		/// <value>
		/// The feeder loaded flag.
		/// </value>
		public IReadOnlyCapWrapper<BoolType> CapFeederLoaded => _feederLoaded ?? (_feederLoaded = new CapWrapper<BoolType>(_source, CapabilityId.CapFeederLoaded, ValueExtensions.ConvertToEnum<BoolType>, readOnly: true));

		/// <summary>
		/// Gets the property to get the image acquired time and date.
		/// </summary>
		/// <value>
		/// The time and date string.
		/// </value>
		public IReadOnlyCapWrapper<string> CapTimeDate => _timedate ?? (_timedate = new CapWrapper<string>(_source, CapabilityId.CapTimeDate, ValueExtensions.ConvertToString, readOnly: true));

		/// <summary>
		/// Gets the supported caps for the current source. This is not supported by all sources.
		/// </summary>
		/// <value>
		/// The supported caps.
		/// </value>
		public IReadOnlyCapWrapper<CapabilityId> CapSupportedCaps => _supportedCaps ?? (_supportedCaps = new CapWrapper<CapabilityId>(_source, CapabilityId.CapSupportedCaps, (object value) => value.ConvertToEnum<CapabilityId>(tryUpperWord: false), readOnly: true));

		/// <summary>
		/// Gets the extended caps for the current source.
		/// </summary>
		/// <value>
		/// The extended caps.
		/// </value>
		public ICapWrapper<CapabilityId> CapExtendedCaps => _extendedCaps ?? (_extendedCaps = new CapWrapper<CapabilityId>(_source, CapabilityId.CapExtendedCaps, (object value) => value.ConvertToEnum<CapabilityId>(tryUpperWord: false), (CapabilityId value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with auto feed page flag for the current source.
		/// </summary>
		/// <value>
		/// The auto feed flag.
		/// </value>
		public ICapWrapper<BoolType> CapAutoFeed => _autoFeed ?? (_autoFeed = new CapWrapper<BoolType>(_source, CapabilityId.CapAutoFeed, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with clear page flag for the current source.
		/// </summary>
		/// <value>
		/// The clear page flag.
		/// </value>
		public ICapWrapper<BoolType> CapClearPage => _clearPage ?? (_clearPage = new CapWrapper<BoolType>(_source, CapabilityId.CapClearPage, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with feed page flag for the current source.
		/// </summary>
		/// <value>
		/// The feed page flag.
		/// </value>
		public ICapWrapper<BoolType> CapFeedPage => _feedPage ?? (_feedPage = new CapWrapper<BoolType>(_source, CapabilityId.CapFeedPage, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with rewind page flag for the current source.
		/// </summary>
		/// <value>
		/// The rewind page flag.
		/// </value>
		public ICapWrapper<BoolType> CapRewindPage => _rewindPage ?? (_rewindPage = new CapWrapper<BoolType>(_source, CapabilityId.CapRewindPage, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with indicators flag for the current source.
		/// </summary>
		/// <value>
		/// The indicators flag.
		/// </value>
		public ICapWrapper<BoolType> CapIndicators => _indicators ?? (_indicators = new CapWrapper<BoolType>(_source, CapabilityId.CapIndicators, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with paper sensor flag for the current source.
		/// </summary>
		/// <value>
		/// The paper sensor flag.
		/// </value>
		public IReadOnlyCapWrapper<BoolType> CapPaperDetectable => _paperDetectable ?? (_paperDetectable = new CapWrapper<BoolType>(_source, CapabilityId.CapPaperDetectable, ValueExtensions.ConvertToEnum<BoolType>, readOnly: true));

		/// <summary>
		/// Gets the property to work with UI controllable flag for the current source.
		/// </summary>
		/// <value>
		/// The UI controllable flag.
		/// </value>
		public IReadOnlyCapWrapper<BoolType> CapUIControllable => _uiControllable ?? (_uiControllable = new CapWrapper<BoolType>(_source, CapabilityId.CapUIControllable, ValueExtensions.ConvertToEnum<BoolType>, readOnly: true));

		/// <summary>
		/// Gets the property to work with devince online flag for the current source.
		/// </summary>
		/// <value>
		/// The devince online flag.
		/// </value>
		public IReadOnlyCapWrapper<BoolType> CapDeviceOnline => _devOnline ?? (_devOnline = new CapWrapper<BoolType>(_source, CapabilityId.CapDeviceOnline, ValueExtensions.ConvertToEnum<BoolType>, readOnly: true));

		/// <summary>
		/// Gets the property to work with auto scan page flag for the current source.
		/// </summary>
		/// <value>
		/// The auto scan flag.
		/// </value>
		public ICapWrapper<BoolType> CapAutoScan => _autoScan ?? (_autoScan = new CapWrapper<BoolType>(_source, CapabilityId.CapAutoScan, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with thumbnails enabled flag for the current source.
		/// </summary>
		/// <value>
		/// The thumbnails enabled flag.
		/// </value>
		public ICapWrapper<BoolType> CapThumbnailsEnabled => _thumbsEnabled ?? (_thumbsEnabled = new CapWrapper<BoolType>(_source, CapabilityId.CapThumbnailsEnabled, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to see what's the duplex mode for the current source.
		/// </summary>
		/// <value>
		/// The duplex mode.
		/// </value>
		public IReadOnlyCapWrapper<Duplex> CapDuplex => _duplex ?? (_duplex = new CapWrapper<Duplex>(_source, CapabilityId.CapDuplex, ValueExtensions.ConvertToEnum<Duplex>, readOnly: true));

		/// <summary>
		/// Gets the property to work with duplex enabled flag for the current source.
		/// </summary>
		/// <value>
		/// The duplex enabled flag.
		/// </value>
		public ICapWrapper<BoolType> CapDuplexEnabled => _duplexEnabled ?? (_duplexEnabled = new CapWrapper<BoolType>(_source, CapabilityId.CapDuplexEnabled, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to see whether device supports UI only flag (no transfer).
		/// </summary>
		/// <value>
		/// The UI only flag.
		/// </value>
		public IReadOnlyCapWrapper<BoolType> CapEnableDSUIOnly => _dsUIonly ?? (_dsUIonly = new CapWrapper<BoolType>(_source, CapabilityId.CapEnableDSUIOnly, ValueExtensions.ConvertToEnum<BoolType>, readOnly: true));

		/// <summary>
		/// Gets the property to see whether device supports custom data triplets.
		/// </summary>
		/// <value>
		/// The custom data flag.
		/// </value>
		public IReadOnlyCapWrapper<BoolType> CapCustomDSData => _dsData ?? (_dsData = new CapWrapper<BoolType>(_source, CapabilityId.CapCustomDSData, ValueExtensions.ConvertToEnum<BoolType>, readOnly: true));

		/// <summary>
		/// Gets the property to work with endorser for the current source.
		/// </summary>
		/// <value>
		/// The endorser option.
		/// </value>
		public ICapWrapper<uint> CapEndorser => _endorser ?? (_endorser = new CapWrapper<uint>(_source, CapabilityId.CapEndorser, ValueExtensions.ConvertToEnum<uint>, (uint value) => new TWOneValue
		{
			Item = value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with job control option for the current source.
		/// </summary>
		/// <value>
		/// The job control option.
		/// </value>
		public ICapWrapper<JobControl> CapJobControl => _jobControl ?? (_jobControl = new CapWrapper<JobControl>(_source, CapabilityId.CapJobControl, ValueExtensions.ConvertToEnum<JobControl>, (JobControl value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with alarms for the current source.
		/// </summary>
		/// <value>
		/// The alarms.
		/// </value>
		public ICapWrapper<AlarmType> CapAlarms => _alarms ?? (_alarms = new CapWrapper<AlarmType>(_source, CapabilityId.CapAlarms, ValueExtensions.ConvertToEnum<AlarmType>, (AlarmType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with alarm volume for the current source.
		/// </summary>
		/// <value>
		/// The alarm volume.
		/// </value>
		public ICapWrapper<int> CapAlarmVolume => _alarmVolume ?? (_alarmVolume = new CapWrapper<int>(_source, CapabilityId.CapAlarmVolume, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Int32
		}));

		/// <summary>
		/// Gets the property to work with auto capture count for the current source.
		/// </summary>
		/// <value>
		/// The auto capture count.
		/// </value>
		public ICapWrapper<int> CapAutomaticCapture => _autoCapture ?? (_autoCapture = new CapWrapper<int>(_source, CapabilityId.CapAutomaticCapture, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Int32
		}));

		/// <summary>
		/// Gets the property to work with the time before first capture (milliseconds) for the current source.
		/// </summary>
		/// <value>
		/// The time before first capture.
		/// </value>
		public ICapWrapper<int> CapTimeBeforeFirstCapture => _timeBeforeCap ?? (_timeBeforeCap = new CapWrapper<int>(_source, CapabilityId.CapTimeBeforeFirstCapture, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Int32
		}));

		/// <summary>
		/// Gets the property to work with the time between captures (milliseconds) for the current source.
		/// </summary>
		/// <value>
		/// The time between captures.
		/// </value>
		public ICapWrapper<int> CapTimeBetweenCaptures => _timeBetweenCap ?? (_timeBetweenCap = new CapWrapper<int>(_source, CapabilityId.CapTimeBetweenCaptures, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Int32
		}));

		/// <summary>
		/// Gets the property to work with the clear buffers option for the current source.
		/// </summary>
		/// <value>
		/// The clear buffers option.
		/// </value>
		public ICapWrapper<ClearBuffer> CapClearBuffers => _clearBuff ?? (_clearBuff = new CapWrapper<ClearBuffer>(_source, CapabilityId.CapClearBuffers, ValueExtensions.ConvertToEnum<ClearBuffer>, (ClearBuffer value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with the max buffered pages for the current source.
		/// </summary>
		/// <value>
		/// The max batch buffered pages.
		/// </value>
		public ICapWrapper<uint> CapMaxBatchBuffers => _maxBatchBuff ?? (_maxBatchBuff = new CapWrapper<uint>(_source, CapabilityId.CapMaxBatchBuffers, ValueExtensions.ConvertToEnum<uint>, (uint value) => new TWOneValue
		{
			Item = value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with the device's time and date.
		/// </summary>
		/// <value>
		/// The device time and date.
		/// </value>
		public ICapWrapper<string> CapDeviceTimeDate => _devTimeDate ?? (_devTimeDate = new CapWrapper<string>(_source, CapabilityId.CapDeviceTimeDate, ValueExtensions.ConvertToString, (string value) =>
		{
			using TWCapability capability = new TWCapability(CapabilityId.CapDeviceTimeDate, value, ItemType.String32);
			return _source.DGControl.Capability.Set(capability);
		}));

		/// <summary>
		/// Gets the property to see current device's power supply.
		/// </summary>
		/// <value>
		/// The power supply indicator.
		/// </value>
		public IReadOnlyCapWrapper<PowerSupply> CapPowerSupply => _powerSup ?? (_powerSup = new CapWrapper<PowerSupply>(_source, CapabilityId.CapPowerSupply, ValueExtensions.ConvertToEnum<PowerSupply>, readOnly: true));

		/// <summary>
		/// Gets the property to see whether device supports camera preview UI flag.
		/// </summary>
		/// <value>
		/// The camera preview UI flag.
		/// </value>
		public IReadOnlyCapWrapper<BoolType> CapCameraPreviewUI => _camPreviewUI ?? (_camPreviewUI = new CapWrapper<BoolType>(_source, CapabilityId.CapCameraPreviewUI, ValueExtensions.ConvertToEnum<BoolType>, readOnly: true));

		/// <summary>
		/// Gets the property to work with the reported device events for the current source.
		/// </summary>
		/// <value>
		/// The reported device events.
		/// </value>
		public ICapWrapper<NTwain.Data.DeviceEvent> CapDeviceEvent => _devEvent ?? (_devEvent = new CapWrapper<NTwain.Data.DeviceEvent>(_source, CapabilityId.CapDeviceEvent, ValueExtensions.ConvertToEnum<NTwain.Data.DeviceEvent>, (NTwain.Data.DeviceEvent value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property for device serial number.
		/// </summary>
		/// <value>
		/// The device serial number.
		/// </value>
		public IReadOnlyCapWrapper<string> CapSerialNumber => _serialNo ?? (_serialNo = new CapWrapper<string>(_source, CapabilityId.CapSerialNumber, ValueExtensions.ConvertToString, readOnly: true));

		/// <summary>
		/// Gets the property to work with printer list for the current source.
		/// </summary>
		/// <value>
		/// The printer list.
		/// </value>
		public ICapWrapper<Printer> CapPrinter => _printer ?? (_printer = new CapWrapper<Printer>(_source, CapabilityId.CapPrinter, ValueExtensions.ConvertToEnum<Printer>, (Printer value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with printer enabled flag.
		/// </summary>
		/// <value>
		/// The printer enabled flag.
		/// </value>
		public ICapWrapper<BoolType> CapPrinterEnabled => _printerEnabled ?? (_printerEnabled = new CapWrapper<BoolType>(_source, CapabilityId.CapPrinterEnabled, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with the starting printer index for the current source.
		/// </summary>
		/// <value>
		/// The printer index.
		/// </value>
		public ICapWrapper<int> CapPrinterIndex => _printerIndex ?? (_printerIndex = new CapWrapper<int>(_source, CapabilityId.CapPrinterIndex, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with printer mode for the current source.
		/// </summary>
		/// <value>
		/// The printer mode.
		/// </value>
		public ICapWrapper<PrinterMode> CapPrinterMode => _printerMode ?? (_printerMode = new CapWrapper<PrinterMode>(_source, CapabilityId.CapPrinterMode, ValueExtensions.ConvertToEnum<PrinterMode>, (PrinterMode value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Specifies the string(s) that are to be used in the string component when the current <see cref="P:NTwain.Capabilities.CapPrinter" />
		/// device is enabled.
		/// </summary>
		/// <value>
		/// The printer string.
		/// </value>
		public ICapWrapper<string> CapPrinterString => _printerString ?? (_printerString = new CapWrapper<string>(_source, CapabilityId.CapPrinterString, ValueExtensions.ConvertToString, (string value) =>
		{
			using TWCapability capability = new TWCapability(CapabilityId.CapPrinterString, value, ItemType.String255);
			return _source.DGControl.Capability.Set(capability);
		}));

		/// <summary>
		/// Specifies the string that shall be used as the current <see cref="P:NTwain.Capabilities.CapPrinter" /> device’s suffix.
		/// </summary>
		/// <value>
		/// The printer suffix string.
		/// </value>
		public ICapWrapper<string> CapPrinterSuffix => _printerSuffix ?? (_printerSuffix = new CapWrapper<string>(_source, CapabilityId.CapPrinterSuffix, ValueExtensions.ConvertToString, (string value) =>
		{
			using TWCapability capability = new TWCapability(CapabilityId.CapPrinterSuffix, value, ItemType.String255);
			return _source.DGControl.Capability.Set(capability);
		}));

		/// <summary>
		/// Gets the property to work with string data language for the current source.
		/// </summary>
		/// <value>
		/// The language.
		/// </value>
		public ICapWrapper<Language> CapLanguage => _language ?? (_language = new CapWrapper<Language>(_source, CapabilityId.CapLanguage, ValueExtensions.ConvertToEnum<Language>, (Language value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with feeder alignment for the current source.
		/// </summary>
		/// <value>
		/// The feeder alignment.
		/// </value>
		public ICapWrapper<FeederAlignment> CapFeederAlignment => _feedAlign ?? (_feedAlign = new CapWrapper<FeederAlignment>(_source, CapabilityId.CapFeederAlignment, ValueExtensions.ConvertToEnum<FeederAlignment>, (FeederAlignment value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with feeder order for the current source.
		/// </summary>
		/// <value>
		/// The feeder order.
		/// </value>
		public ICapWrapper<FeederOrder> CapFeederOrder => _feedOrder ?? (_feedOrder = new CapWrapper<FeederOrder>(_source, CapabilityId.CapFeederOrder, ValueExtensions.ConvertToEnum<FeederOrder>, (FeederOrder value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to see whether device supports reacquire flag.
		/// </summary>
		/// <value>
		/// The reacquire flag.
		/// </value>
		public IReadOnlyCapWrapper<BoolType> CapReacquireAllowed => _reacuireAllow ?? (_reacuireAllow = new CapWrapper<BoolType>(_source, CapabilityId.CapReacquireAllowed, ValueExtensions.ConvertToEnum<BoolType>, readOnly: true));

		/// <summary>
		/// Gets the property to see the remaining battery power for the device.
		/// </summary>
		/// <value>
		/// The battery minutes.
		/// </value>
		public IReadOnlyCapWrapper<int> CapBatteryMinutes => _battMinutes ?? (_battMinutes = new CapWrapper<int>(_source, CapabilityId.CapBatteryMinutes, ValueExtensions.ConvertToEnum<int>, readOnly: true));

		/// <summary>
		/// Gets the property to see the remaining battery percentage for the device.
		/// </summary>
		/// <value>
		/// The battery percentage.
		/// </value>
		public IReadOnlyCapWrapper<int> CapBatteryPercentage => _battPercent ?? (_battPercent = new CapWrapper<int>(_source, CapabilityId.CapBatteryPercentage, ValueExtensions.ConvertToEnum<int>, readOnly: true));

		/// <summary>
		/// Gets the property to work with camera side for the current source.
		/// </summary>
		/// <value>
		/// The camera side.
		/// </value>
		public ICapWrapper<CameraSide> CapCameraSide => _camSide ?? (_camSide = new CapWrapper<CameraSide>(_source, CapabilityId.CapCameraSide, ValueExtensions.ConvertToEnum<CameraSide>, (CameraSide value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with segmentation setting for the current source.
		/// </summary>
		/// <value>
		/// The segmentation setting.
		/// </value>
		public ICapWrapper<Segmented> CapSegmented => _segmented ?? (_segmented = new CapWrapper<Segmented>(_source, CapabilityId.CapSegmented, ValueExtensions.ConvertToEnum<Segmented>, (Segmented value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with camera enabled flag.
		/// </summary>
		/// <value>
		/// The camera enabled flag.
		/// </value>
		public ICapWrapper<BoolType> CapCameraEnabled => _camEnabled ?? (_camEnabled = new CapWrapper<BoolType>(_source, CapabilityId.CapCameraEnabled, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with camera order for the current source.
		/// </summary>
		/// <value>
		/// The camera order setting.
		/// </value>
		public ICapWrapper<PixelType> CapCameraOrder => _camOrder ?? (_camOrder = new CapWrapper<PixelType>(_source, CapabilityId.CapCameraOrder, ValueExtensions.ConvertToEnum<PixelType>, (PixelType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with check scanning support flag.
		/// </summary>
		/// <value>
		/// The check scanning support flag.
		/// </value>
		public ICapWrapper<BoolType> CapMicrEnabled => _micrEnabled ?? (_micrEnabled = new CapWrapper<BoolType>(_source, CapabilityId.CapMicrEnabled, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with feeder prep flag.
		/// </summary>
		/// <value>
		/// The feeder prep flag.
		/// </value>
		public ICapWrapper<BoolType> CapFeederPrep => _feederPrep ?? (_feederPrep = new CapWrapper<BoolType>(_source, CapabilityId.CapFeederPrep, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property to work with feeder pocket for the current source.
		/// </summary>
		/// <value>
		/// The feeder pocket setting.
		/// </value>
		public ICapWrapper<FeederPocket> CapFeederPocket => _feedPocket ?? (_feedPocket = new CapWrapper<FeederPocket>(_source, CapabilityId.CapFeederPocket, ValueExtensions.ConvertToEnum<FeederPocket>, (FeederPocket value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with auto-sense medium (paper source) flag.
		/// </summary>
		/// <value>
		/// The auto-sense medium flag.
		/// </value>
		public ICapWrapper<BoolType> CapAutomaticSenseMedium => _autoMedium ?? (_autoMedium = new CapWrapper<BoolType>(_source, CapabilityId.CapAutomaticSenseMedium, ValueExtensions.ConvertToEnum<BoolType>, (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		}));

		/// <summary>
		/// Gets the property for device interface guid.
		/// </summary>
		/// <value>
		/// The device interface guid.
		/// </value>
		public IReadOnlyCapWrapper<string> CapCustomInterfaceGuid => _custGuid ?? (_custGuid = new CapWrapper<string>(_source, CapabilityId.CapCustomInterfaceGuid, ValueExtensions.ConvertToString, readOnly: true));

		/// <summary>
		/// Gets the supported caps for unique segments for the current source.
		/// </summary>
		/// <value>
		/// The supported caps for unique segments.
		/// </value>
		public IReadOnlyCapWrapper<CapabilityId> CapSupportedCapsSegmentUnique => _supportedCapsUnique ?? (_supportedCapsUnique = new CapWrapper<CapabilityId>(_source, CapabilityId.CapSupportedCapsSegmentUnique, (object value) => value.ConvertToEnum<CapabilityId>(tryUpperWord: false), readOnly: true));

		/// <summary>
		/// Gets the supported caps for supported DATs.
		/// </summary>
		/// <value>
		/// The supported DATs.
		/// </value>
		public IReadOnlyCapWrapper<uint> CapSupportedDATs => _supportedDat ?? (_supportedDat = new CapWrapper<uint>(_source, CapabilityId.CapSupportedDATs, ValueExtensions.ConvertToEnum<uint>, readOnly: true));

		/// <summary>
		/// Gets the property to work with double feed detection option for the current source.
		/// </summary>
		/// <value>
		/// The double feed detection option.
		/// </value>
		public ICapWrapper<DoubleFeedDetection> CapDoubleFeedDetection => _dblFeedDetect ?? (_dblFeedDetect = new CapWrapper<DoubleFeedDetection>(_source, CapabilityId.CapDoubleFeedDetection, ValueExtensions.ConvertToEnum<DoubleFeedDetection>, (DoubleFeedDetection value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with double feed detection length for the current source.
		/// </summary>
		/// <value>
		/// The double feed detection length.
		/// </value>
		public ICapWrapper<TWFix32> CapDoubleFeedDetectionLength => _dblFeedLength ?? (_dblFeedLength = new CapWrapper<TWFix32>(_source, CapabilityId.CapDoubleFeedDetectionLength, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with double feed detection sensitivity for the current source.
		/// </summary>
		/// <value>
		/// The double feed detection sensitivity.
		/// </value>
		public ICapWrapper<DoubleFeedDetectionSensitivity> CapDoubleFeedDetectionSensitivity => _dblFeedSensitivity ?? (_dblFeedSensitivity = new CapWrapper<DoubleFeedDetectionSensitivity>(_source, CapabilityId.CapDoubleFeedDetectionSensitivity, ValueExtensions.ConvertToEnum<DoubleFeedDetectionSensitivity>, (DoubleFeedDetectionSensitivity value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with double feed detection response for the current source.
		/// </summary>
		/// <value>
		/// The double feed detection response.
		/// </value>
		public ICapWrapper<DoubleFeedDetectionResponse> CapDoubleFeedDetectionResponse => _dblFeedResp ?? (_dblFeedResp = new CapWrapper<DoubleFeedDetectionResponse>(_source, CapabilityId.CapDoubleFeedDetectionResponse, ValueExtensions.ConvertToEnum<DoubleFeedDetectionResponse>, (DoubleFeedDetectionResponse value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with paper handling option for the current source.
		/// </summary>
		/// <value>
		/// The paper handling option.
		/// </value>
		public ICapWrapper<PaperHandling> CapPaperHandling => _paperHandling ?? (_paperHandling = new CapWrapper<PaperHandling>(_source, CapabilityId.CapPaperHandling, ValueExtensions.ConvertToEnum<PaperHandling>, (PaperHandling value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with diplayed indicators for the current source.
		/// </summary>
		/// <value>
		/// The diplayed indicators.
		/// </value>
		public ICapWrapper<IndicatorsMode> CapIndicatorsMode => _indicatorMode ?? (_indicatorMode = new CapWrapper<IndicatorsMode>(_source, CapabilityId.CapIndicatorsMode, ValueExtensions.ConvertToEnum<IndicatorsMode>, (IndicatorsMode value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		}));

		/// <summary>
		/// Gets the property to work with printer y-offset for the current source.
		/// </summary>
		/// <value>
		/// The printer y-offset.
		/// </value>
		public ICapWrapper<TWFix32> CapPrinterVerticalOffset => _printVOffset ?? (_printVOffset = new CapWrapper<TWFix32>(_source, CapabilityId.CapPrinterVerticalOffset, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue()));

		/// <summary>
		/// Gets the property to work with camera power down time (seconds) for the current source.
		/// </summary>
		/// <value>
		/// The camera power down time.
		/// </value>
		public ICapWrapper<int> CapPowerSaveTime => _powerSaveTime ?? (_powerSaveTime = new CapWrapper<int>(_source, CapabilityId.CapPowerSaveTime, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Int32
		}));

		/// <summary>
		/// Gets the property to work with printer character rotation for the current source.
		/// </summary>
		/// <value>
		/// The printer character rotation.
		/// </value>
		public ICapWrapper<int> CapPrinterCharRotation => _printCharRot ?? (_printCharRot = new CapWrapper<int>(_source, CapabilityId.CapPrinterCharRotation, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with printer font style for the current source.
		/// </summary>
		/// <value>
		/// The printer font style.
		/// </value>
		public ICapWrapper<PrinterFontStyle> CapPrinterFontStyle => _printFontStyle ?? (_printFontStyle = new CapWrapper<PrinterFontStyle>(_source, CapabilityId.CapPrinterFontStyle, ValueExtensions.ConvertToEnum<PrinterFontStyle>, readOnly: false));

		/// <summary>
		/// Set the character to be used for filling the leading digits before the counter value if the
		/// counter digits are fewer than <see cref="P:NTwain.Capabilities.CapPrinterIndexNumDigits" />.
		/// </summary>
		/// <value>
		/// The printer leading string.
		/// </value>
		public ICapWrapper<string> CapPrinterIndexLeadChar => _printerIdxLeadChar ?? (_printerIdxLeadChar = new CapWrapper<string>(_source, CapabilityId.CapPrinterIndexLeadChar, ValueExtensions.ConvertToString, (string value) =>
		{
			using TWCapability capability = new TWCapability(CapabilityId.CapPrinterIndexLeadChar, value, ItemType.String32);
			return _source.DGControl.Capability.Set(capability);
		}));

		/// <summary>
		/// Gets the property to work with printer index max value for the current source.
		/// </summary>
		/// <value>
		/// The printer index max value.
		/// </value>
		public ICapWrapper<uint> CapPrinterIndexMaxValue => _printIdxMax ?? (_printIdxMax = new CapWrapper<uint>(_source, CapabilityId.CapPrinterIndexMaxValue, ValueExtensions.ConvertToEnum<uint>, (uint value) => new TWOneValue
		{
			Item = value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with printer number digits value for the current source.
		/// </summary>
		/// <value>
		/// The printer number digits value.
		/// </value>
		public ICapWrapper<int> CapPrinterIndexNumDigits => _printNumDigit ?? (_printNumDigit = new CapWrapper<int>(_source, CapabilityId.CapPrinterIndexNumDigits, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with printer index step value for the current source.
		/// </summary>
		/// <value>
		/// The printer index step value.
		/// </value>
		public ICapWrapper<int> CapPrinterIndexStep => _printIdxStep ?? (_printIdxStep = new CapWrapper<int>(_source, CapabilityId.CapPrinterIndexStep, ValueExtensions.ConvertToEnum<int>, (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt32
		}));

		/// <summary>
		/// Gets the property to work with printer index trigger for the current source.
		/// </summary>
		/// <value>
		/// The printer index trigger.
		/// </value>
		public ICapWrapper<PrinterIndexTrigger> CapPrinterIndexTrigger => _printIdxTrig ?? (_printIdxTrig = new CapWrapper<PrinterIndexTrigger>(_source, CapabilityId.CapPrinterIndexTrigger, ValueExtensions.ConvertToEnum<PrinterIndexTrigger>, readOnly: false));

		/// <summary>
		/// Gets the next print values.
		/// </summary>
		/// <value>
		/// The next print values.
		/// </value>
		public IReadOnlyCapWrapper<string> CapPrinterStringPreview => _printPreview ?? (_printPreview = new CapWrapper<string>(_source, CapabilityId.CapPrinterStringPreview, ValueExtensions.ConvertToString, readOnly: true));

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Capabilities" /> class.
		/// </summary>
		/// <param name="source">The source.</param>
		public Capabilities(IDataSource source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			_source = source;
		}

		/// <summary>
		/// Gets the actual supported operations for a capability. This is not supported by all sources.
		/// </summary>
		/// <param name="capabilityId">The capability id.</param>
		/// <returns></returns>
		public QuerySupports? QuerySupport(CapabilityId capabilityId)
		{
			QuerySupports? result = null;
			using (TWCapability tWCapability = new TWCapability(capabilityId))
			{
				tWCapability.ContainerType = ContainerType.OneValue;
				if (_source.DGControl.Capability.QuerySupport(tWCapability) == ReturnCode.Success)
				{
					CapabilityReader capabilityReader = CapabilityReader.ReadValue(tWCapability);
					if (capabilityReader.ContainerType == ContainerType.OneValue)
					{
						result = capabilityReader.OneValue.ConvertToEnum<QuerySupports>();
						return result;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the current value for a capability.
		/// </summary>
		/// <param name="capabilityId">The capability id.</param>
		/// <returns></returns>
		public object GetCurrent(CapabilityId capabilityId)
		{
			using (TWCapability capability = new TWCapability(capabilityId))
			{
				if (_source.DGControl.Capability.GetCurrent(capability) == ReturnCode.Success)
				{
					CapabilityReader capabilityReader = CapabilityReader.ReadValue(capability);
					switch (capabilityReader.ContainerType)
					{
					case ContainerType.Enum:
						if (capabilityReader.CollectionValues != null && capabilityReader.CollectionValues.Count > capabilityReader.EnumCurrentIndex)
						{
							return capabilityReader.CollectionValues[capabilityReader.EnumCurrentIndex];
						}
						break;
					case ContainerType.OneValue:
						return capabilityReader.OneValue;
					case ContainerType.Range:
						return capabilityReader.RangeCurrentValue;
					case ContainerType.Array:
						if (capabilityReader.CollectionValues != null)
						{
							return capabilityReader.CollectionValues.FirstOrDefault();
						}
						break;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the default value for a capability.
		/// </summary>
		/// <param name="capabilityId">The capability id.</param>
		/// <returns></returns>
		public object GetDefault(CapabilityId capabilityId)
		{
			using (TWCapability capability = new TWCapability(capabilityId))
			{
				if (_source.DGControl.Capability.GetDefault(capability) == ReturnCode.Success)
				{
					CapabilityReader capabilityReader = CapabilityReader.ReadValue(capability);
					switch (capabilityReader.ContainerType)
					{
					case ContainerType.Enum:
						if (capabilityReader.CollectionValues != null && capabilityReader.CollectionValues.Count > capabilityReader.EnumDefaultIndex)
						{
							return capabilityReader.CollectionValues[capabilityReader.EnumDefaultIndex];
						}
						break;
					case ContainerType.OneValue:
						return capabilityReader.OneValue;
					case ContainerType.Range:
						return capabilityReader.RangeDefaultValue;
					case ContainerType.Array:
						if (capabilityReader.CollectionValues != null)
						{
							return capabilityReader.CollectionValues.FirstOrDefault();
						}
						break;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// A general method that tries to get capability values from current <see cref="T:NTwain.DataSource" />.
		/// </summary>
		/// <param name="capabilityId">The capability id.</param>
		/// <returns></returns>
		public IEnumerable<object> GetValues(CapabilityId capabilityId)
		{
			using (TWCapability capability = new TWCapability(capabilityId))
			{
				if (_source.DGControl.Capability.Get(capability) == ReturnCode.Success)
				{
					return CapabilityReader.ReadValue(capability).EnumerateCapValues();
				}
			}
			return Enumerable.Empty<object>();
		}

		/// <summary>
		/// Gets all the possible values of this capability without expanding.
		/// This may be required to work with large range values that cannot be safely enumerated
		/// with <see cref="M:NTwain.Capabilities.GetValues(NTwain.Data.CapabilityId)" />.
		/// </summary>
		/// <param name="capabilityId">The capability id.</param>
		/// <returns></returns>
		public CapabilityReader GetValuesRaw(CapabilityId capabilityId)
		{
			using (TWCapability capability = new TWCapability(capabilityId))
			{
				if (_source.DGControl.Capability.Get(capability) == ReturnCode.Success)
				{
					return CapabilityReader.ReadValue(capability);
				}
			}
			return new CapabilityReader();
		}

		/// <summary>
		/// Resets all values and constraint to power-on defaults.
		/// </summary>
		/// <returns></returns>
		public ReturnCode ResetAll()
		{
			using TWCapability capability = new TWCapability(CapabilityId.CapSupportedCaps);
			return _source.DGControl.Capability.ResetAll(capability);
		}

		/// <summary>
		/// Resets the current value to power-on default.
		/// </summary>
		/// <param name="capabilityId">The capability id.</param>
		/// <returns></returns>
		public ReturnCode Reset(CapabilityId capabilityId)
		{
			using TWCapability capability = new TWCapability(capabilityId);
			return _source.DGControl.Capability.Reset(capability);
		}
	}
	public class CapabilitiesExt : Capabilities
	{
		private IDataSource _source;

		private List<CapabilityId> _custom;

		private Func<BoolType, TWOneValue> boolFunc = (BoolType value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.Bool
		};

		private Func<int, TWOneValue> intFunc = (int value) => new TWOneValue
		{
			Item = (uint)value,
			ItemType = ItemType.UInt16
		};

		private Func<byte, TWOneValue> byteFunc = (byte value) => new TWOneValue
		{
			Item = value,
			ItemType = ItemType.UInt8
		};

		private Func<uint, TWOneValue> uintFunc = (uint value) => new TWOneValue
		{
			Item = value,
			ItemType = ItemType.UInt32
		};

		public List<CapabilityId> CustomCapabilities => _custom ?? LoadCustomCapabilities();

		public CapabilitiesExt(IDataSource dataSource)
			: base(dataSource)
		{
			_source = dataSource;
		}

		private List<CapabilityId> LoadCustomCapabilities()
		{
			List<CapabilityId> list = new List<CapabilityId>();
			foreach (CapabilityId value in CapSupportedCaps.GetValues())
			{
				string name = value.ToString();
				if (GetType().GetProperty(name) == null)
				{
					list.Add(value);
				}
			}
			return list;
		}

		public object GetCap<TValue>(CapabilityId Capability)
		{
			QuerySupports? querySupports = QuerySupport(Capability);
			bool flag = true;
			if (querySupports.HasValue && ((byte)querySupports.Value & 4) != 0)
			{
				flag = false;
			}
			if (typeof(TValue) == typeof(string))
			{
				return new CapWrapper<string>(_source, Capability, ValueExtensions.ConvertToString, flag);
			}
			if (typeof(TValue) == typeof(int))
			{
				if (flag)
				{
					return new CapWrapper<int>(_source, Capability, ValueExtensions.ConvertToEnum<int>, readOnly: true);
				}
				return new CapWrapper<int>(_source, Capability, ValueExtensions.ConvertToEnum<int>, intFunc);
			}
			if (typeof(TValue) == typeof(byte))
			{
				if (flag)
				{
					return new CapWrapper<byte>(_source, Capability, ValueExtensions.ConvertToEnum<byte>, readOnly: true);
				}
				return new CapWrapper<byte>(_source, Capability, ValueExtensions.ConvertToEnum<byte>, byteFunc);
			}
			if (typeof(TValue) == typeof(uint))
			{
				if (flag)
				{
					return new CapWrapper<uint>(_source, Capability, ValueExtensions.ConvertToEnum<uint>, readOnly: true);
				}
				return new CapWrapper<uint>(_source, Capability, ValueExtensions.ConvertToEnum<uint>, uintFunc);
			}
			if (typeof(TValue) == typeof(TWFrame))
			{
				return new CapWrapper<TWFrame>(_source, Capability, ValueExtensions.ConvertToFrame, flag);
			}
			if (typeof(TValue) == typeof(BoolType))
			{
				if (flag)
				{
					return new CapWrapper<BoolType>(_source, Capability, ValueExtensions.ConvertToEnum<BoolType>, readOnly: true);
				}
				return new CapWrapper<BoolType>(_source, Capability, ValueExtensions.ConvertToEnum<BoolType>, boolFunc);
			}
			if (typeof(TValue) == typeof(TWFix32))
			{
				if (flag)
				{
					return new CapWrapper<TWFix32>(_source, Capability, ValueExtensions.ConvertToFix32, readOnly: true);
				}
				return new CapWrapper<TWFix32>(_source, Capability, ValueExtensions.ConvertToFix32, (TWFix32 value) => value.ToOneValue());
			}
			throw new Exception($"Unknown defintion for type of {typeof(TValue)} in GetCap");
		}
	}
	/// <summary>
	/// The one-stop class for reading raw TWAIN cap values from the cap container.
	/// This contains all the properties for the 4 container types.
	/// </summary>
	public class CapabilityReader
	{
		/// <summary>
		/// Gets the underlying container type. 
		/// </summary>
		/// <value>
		/// The container.
		/// </value>
		public ContainerType ContainerType { get; private set; }

		/// <summary>
		/// Gets the type of the TWAIN value.
		/// </summary>
		/// <value>
		/// The type of the value.
		/// </value>
		public ItemType ItemType { get; private set; }

		/// <summary>
		/// Gets the one value if container is <see cref="F:NTwain.Data.ContainerType.OneValue" />.
		/// </summary>
		/// <value>
		/// The one value.
		/// </value>
		public object OneValue { get; private set; }

		/// <summary>
		/// Gets the collection values if container is <see cref="F:NTwain.Data.ContainerType.Enum" /> or <see cref="F:NTwain.Data.ContainerType.Array" /> .
		/// </summary>
		/// <value>
		/// The collection values.
		/// </value>
		public IList<object> CollectionValues { get; private set; }

		/// <summary>
		/// Gets the current value index if container is <see cref="F:NTwain.Data.ContainerType.Enum" />.
		/// </summary>
		public int EnumCurrentIndex { get; private set; }

		/// <summary>
		/// Gets the default value index if container is <see cref="F:NTwain.Data.ContainerType.Enum" />.
		/// </summary>
		public int EnumDefaultIndex { get; private set; }

		/// <summary>
		/// Gets the current value if container is <see cref="F:NTwain.Data.ContainerType.Range" />.
		/// </summary>
		/// <value>
		/// The range current value.
		/// </value>
		public object RangeCurrentValue { get; private set; }

		/// <summary>
		/// Gets the default value if container is <see cref="F:NTwain.Data.ContainerType.Range" />.
		/// </summary>
		/// <value>
		/// The range default value.
		/// </value>
		public object RangeDefaultValue { get; private set; }

		/// <summary>
		/// The least positive/most negative value of the range.
		/// </summary>
		/// <value>
		/// The range minimum value.
		/// </value>
		public object RangeMinValue { get; private set; }

		/// <summary>
		/// The most positive/least negative value of the range.
		/// </summary>
		/// <value>
		/// The range maximum value.
		/// </value>
		public object RangeMaxValue { get; private set; }

		/// <summary>
		/// The delta between two adjacent values of the range.
		/// e.g. Item2 - Item1 = StepSize;
		/// </summary>
		/// <value>
		/// The size of the range step.
		/// </value>
		public object RangeStepSize { get; private set; }

		/// <summary>
		/// Gets the number of range values if range is expanded.
		/// You should check for this before expanding range type containers to prevent
		/// possible <see cref="T:System.OutOfMemoryException" />.
		/// </summary>
		/// <value>
		/// The range count.
		/// </value>
		public int RangeCount { get; private set; }

		/// <summary>
		/// Reads the value from a <see cref="T:NTwain.Data.TWCapability" /> that was returned
		/// from a TWAIN source.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		/// <exception cref="T:System.ArgumentNullException">capability</exception>
		/// <exception cref="T:System.ArgumentException">Capability contains no data.;capability
		/// or
		/// capability</exception>
		public static CapabilityReader ReadValue(TWCapability capability)
		{
			return ReadValue(capability, PlatformInfo.Current.MemoryManager);
		}

		/// <summary>
		/// Reads the value from a <see cref="T:NTwain.Data.TWCapability" /> that was returned
		/// from a TWAIN source.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <param name="memoryManager">The memory manager.</param>
		/// <returns></returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// capability
		/// or
		/// memoryManager
		/// </exception>
		/// <exception cref="T:System.ArgumentException">capability</exception>
		public static CapabilityReader ReadValue(TWCapability capability, IMemoryManager memoryManager)
		{
			if (capability == null)
			{
				throw new ArgumentNullException("capability");
			}
			if (memoryManager == null)
			{
				throw new ArgumentNullException("memoryManager");
			}
			if (capability.Container != IntPtr.Zero)
			{
				IntPtr intPtr = IntPtr.Zero;
				try
				{
					intPtr = memoryManager.Lock(capability.Container);
					return capability.ContainerType switch
					{
						ContainerType.Array => new CapabilityReader
						{
							ContainerType = capability.ContainerType
						}.ReadArrayValue(intPtr), 
						ContainerType.Enum => new CapabilityReader
						{
							ContainerType = capability.ContainerType
						}.ReadEnumValue(intPtr), 
						ContainerType.OneValue => new CapabilityReader
						{
							ContainerType = capability.ContainerType
						}.ReadOneValue(intPtr), 
						ContainerType.Range => new CapabilityReader
						{
							ContainerType = capability.ContainerType
						}.ReadRangeValue(intPtr), 
						_ => throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.CapHasBadContainer, capability.Capability, capability.ContainerType), "capability"), 
					};
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						memoryManager.Unlock(capability.Container);
					}
				}
			}
			return new CapabilityReader();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.CapabilityReader" /> class.
		/// </summary>
		public CapabilityReader()
		{
			RangeCount = -1;
		}

		/// <summary>
		/// Don't care what container it is, just enumerate the capability values.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<object> EnumerateCapValues()
		{
			switch (ContainerType)
			{
			case ContainerType.OneValue:
				return EnumerateOneValue();
			case ContainerType.Array:
			case ContainerType.Enum:
				if (CollectionValues != null)
				{
					return CollectionValues;
				}
				break;
			case ContainerType.Range:
				return EnumerateRange();
			}
			return Enumerable.Empty<object>();
		}

		private IEnumerable<object> EnumerateOneValue()
		{
			if (OneValue != null)
			{
				yield return OneValue;
			}
		}

		private IEnumerable<object> EnumerateRange()
		{
			switch (ItemType)
			{
			case ItemType.Fix32:
			{
				TWFix32 min6 = (TWFix32)RangeMinValue;
				TWFix32 max6 = (TWFix32)RangeMaxValue;
				TWFix32 step6 = (TWFix32)RangeStepSize;
				TWFix32 i5 = min6;
				while ((float)i5 >= (float)min6 && (float)i5 <= (float)max6)
				{
					yield return i5;
					i5 = (float)i5 + (float)step6;
				}
				break;
			}
			case ItemType.UInt32:
			{
				uint min2 = (uint)RangeMinValue;
				uint max2 = (uint)RangeMaxValue;
				uint step2 = (uint)RangeStepSize;
				for (uint i2 = min2; i2 >= min2 && i2 <= max2; i2 += step2)
				{
					yield return i2;
				}
				break;
			}
			case ItemType.Int32:
			{
				int min4 = (int)RangeMinValue;
				int max4 = (int)RangeMaxValue;
				int step4 = (int)RangeStepSize;
				for (int j = min4; j >= min4 && j <= max4; j += step4)
				{
					yield return j;
				}
				break;
			}
			case ItemType.UInt16:
			{
				ushort min7 = (ushort)RangeMinValue;
				ushort max7 = (ushort)RangeMaxValue;
				ushort step7 = (ushort)RangeStepSize;
				ushort i6 = min7;
				while (i6 >= min7 && i6 <= max7)
				{
					yield return i6;
					i6 += step7;
				}
				break;
			}
			case ItemType.Int16:
			{
				short min5 = (short)RangeMinValue;
				short max5 = (short)RangeMaxValue;
				short step5 = (short)RangeStepSize;
				short i4 = min5;
				while (i4 >= min5 && i4 <= max5)
				{
					yield return i4;
					i4 += step5;
				}
				break;
			}
			case ItemType.UInt8:
			{
				byte min3 = (byte)RangeMinValue;
				byte max3 = (byte)RangeMaxValue;
				byte step3 = (byte)RangeStepSize;
				byte i3 = min3;
				while (i3 >= min3 && i3 <= max3)
				{
					yield return i3;
					i3 += step3;
				}
				break;
			}
			case ItemType.Int8:
			{
				sbyte min = (sbyte)RangeMinValue;
				sbyte max = (sbyte)RangeMaxValue;
				sbyte step = (sbyte)RangeStepSize;
				sbyte i = min;
				while (i >= min && i <= max)
				{
					yield return i;
					i += step;
				}
				break;
			}
			}
		}

		private CapabilityReader ReadOneValue(IntPtr baseAddr)
		{
			int num = 0;
			ItemType = (ItemType)Marshal.ReadInt16(baseAddr, num);
			num += 2;
			OneValue = baseAddr.ReadValue(ref num, ItemType);
			return this;
		}

		private CapabilityReader ReadArrayValue(IntPtr baseAddr)
		{
			int num = 0;
			ItemType = (ItemType)Marshal.ReadInt16(baseAddr, num);
			num += 2;
			int num2 = Marshal.ReadInt32(baseAddr, num);
			num += 4;
			if (num2 > 0)
			{
				CollectionValues = new object[num2];
				for (int i = 0; i < num2; i++)
				{
					CollectionValues[i] = baseAddr.ReadValue(ref num, ItemType);
				}
			}
			return this;
		}

		private CapabilityReader ReadEnumValue(IntPtr baseAddr)
		{
			int num = 0;
			ItemType = (ItemType)Marshal.ReadInt16(baseAddr, num);
			num += 2;
			int num2 = Marshal.ReadInt32(baseAddr, num);
			num += 4;
			EnumCurrentIndex = Marshal.ReadInt32(baseAddr, num);
			num += 4;
			EnumDefaultIndex = Marshal.ReadInt32(baseAddr, num);
			num += 4;
			if (num2 > 0)
			{
				CollectionValues = new object[num2];
				for (int i = 0; i < num2; i++)
				{
					CollectionValues[i] = baseAddr.ReadValue(ref num, ItemType);
				}
			}
			return this;
		}

		private CapabilityReader ReadRangeValue(IntPtr baseAddr)
		{
			int num = 0;
			ItemType = (ItemType)Marshal.ReadInt16(baseAddr, num);
			num += 2;
			RangeMinValue = baseAddr.ReadValue(ref num, ItemType);
			RangeMaxValue = baseAddr.ReadValue(ref num, ItemType);
			RangeStepSize = baseAddr.ReadValue(ref num, ItemType);
			RangeDefaultValue = baseAddr.ReadValue(ref num, ItemType);
			RangeCurrentValue = baseAddr.ReadValue(ref num, ItemType);
			switch (ItemType)
			{
			case ItemType.Fix32:
			{
				TWFix32 tWFix = (TWFix32)RangeMinValue;
				TWFix32 tWFix2 = (TWFix32)RangeMaxValue;
				TWFix32 tWFix3 = (TWFix32)RangeStepSize;
				RangeCount = (int)(((float)tWFix2 - (float)tWFix) / (float)tWFix3) + 1;
				break;
			}
			case ItemType.UInt32:
			{
				uint num11 = (uint)RangeMinValue;
				uint num12 = (uint)RangeMaxValue;
				uint num13 = (uint)RangeStepSize;
				RangeCount = (int)((num12 - num11) / num13 + 1);
				break;
			}
			case ItemType.Int32:
			{
				int num8 = (int)RangeMinValue;
				int num9 = (int)RangeMaxValue;
				int num10 = (int)RangeStepSize;
				RangeCount = (num9 - num8) / num10 + 1;
				break;
			}
			case ItemType.UInt16:
			{
				ushort num5 = (ushort)RangeMinValue;
				ushort num6 = (ushort)RangeMaxValue;
				ushort num7 = (ushort)RangeStepSize;
				RangeCount = (num6 - num5) / num7 + 1;
				break;
			}
			case ItemType.Int16:
			{
				short num2 = (short)RangeMinValue;
				short num3 = (short)RangeMaxValue;
				short num4 = (short)RangeStepSize;
				RangeCount = (num3 - num2) / num4 + 1;
				break;
			}
			case ItemType.UInt8:
			{
				byte b4 = (byte)RangeMinValue;
				byte b5 = (byte)RangeMaxValue;
				byte b6 = (byte)RangeStepSize;
				RangeCount = (b5 - b4) / b6 + 1;
				break;
			}
			case ItemType.Int8:
			{
				sbyte b = (sbyte)RangeMinValue;
				sbyte b2 = (sbyte)RangeMaxValue;
				sbyte b3 = (sbyte)RangeStepSize;
				RangeCount = (b2 - b) / b3 + 1;
				break;
			}
			}
			return this;
		}
	}
	/// <summary>
	/// Wrapped class for reading/writing a TWAIN capability associated with a <see cref="T:NTwain.DataSource" />.
	/// </summary>
	/// <typeparam name="TValue">The TWAIN type of the value.</typeparam>
	public class CapWrapper<TValue> : ICapWrapper<TValue>, IReadOnlyCapWrapper<TValue>
	{
		private IDataSource _source;

		private Func<object, TValue> _getConvertRoutine;

		private Func<TValue, ReturnCode> _setCustomRoutine;

		private Func<TValue, TWOneValue> _setOneValueFunc;

		private QuerySupports? _supports;

		/// <summary>
		/// Gets the capability.
		/// </summary>
		/// <value>
		/// The capability.
		/// </value>
		public CapabilityId Capability { get; private set; }

		/// <summary>
		/// Gets the supported actions.
		/// </summary>
		/// <value>
		/// The supported actions.
		/// </value>
		public QuerySupports SupportedActions
		{
			get
			{
				CheckSupports();
				return _supports.GetValueOrDefault();
			}
		}

		/// <summary>
		/// Gets a value indicating whether this capability is supported.
		/// </summary>
		/// <value>
		/// <c>true</c> if this capability is supported; otherwise, <c>false</c>.
		/// </value>
		public bool IsSupported => SupportedActions > QuerySupports.None;

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		public bool IsReadOnly { get; private set; }

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.CapWrapper`1.GetValues" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can get values; otherwise, <c>false</c>.
		/// </value>
		public bool CanGet => Supports(QuerySupports.Get);

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.CapWrapper`1.GetDefault" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can get default value; otherwise, <c>false</c>.
		/// </value>
		public bool CanGetDefault => Supports(QuerySupports.GetDefault);

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.CapWrapper`1.GetCurrent" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can get current value; otherwise, <c>false</c>.
		/// </value>
		public bool CanGetCurrent => Supports(QuerySupports.GetCurrent);

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.CapWrapper`1.GetLabel" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can get label; otherwise, <c>false</c>.
		/// </value>
		public bool CanGetLabel => Supports(QuerySupports.GetLabel);

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.CapWrapper`1.GetHelp" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can get help; otherwise, <c>false</c>.
		/// </value>
		public bool CanGetHelp => Supports(QuerySupports.GetHelp);

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.CapWrapper`1.GetLabelEnum" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can get label enum; otherwise, <c>false</c>.
		/// </value>
		public bool CanGetLabelEnum => Supports(QuerySupports.GetLabelEnum);

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.CapWrapper`1.Reset" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can reset; otherwise, <c>false</c>.
		/// </value>
		public bool CanReset => Supports(QuerySupports.Reset);

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.CapWrapper`1.SetValue(`0)" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can set; otherwise, <c>false</c>.
		/// </value>
		public bool CanSet => Supports(QuerySupports.Set);

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.CapWrapper`1.SetConstraint(NTwain.Data.TWOneValue)" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can set constraint; otherwise, <c>false</c>.
		/// </value>
		public bool CanSetConstraint => Supports(QuerySupports.SetConstraint);

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.CapWrapper`1" /> class.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="capability">The capability.</param>
		/// <param name="getConversionRoutine">The value conversion routine in Get methods.</param>
		/// <param name="readOnly">if set to <c>true</c> then make this cap read-only.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// source
		/// or
		/// getConversionRoutine
		/// </exception>
		/// <exception cref="T:System.ArgumentNullException">source
		/// or
		/// getConversionRoutine</exception>
		public CapWrapper(IDataSource source, CapabilityId capability, Func<object, TValue> getConversionRoutine, bool readOnly)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (getConversionRoutine == null)
			{
				throw new ArgumentNullException("getConversionRoutine");
			}
			_source = source;
			_getConvertRoutine = getConversionRoutine;
			IsReadOnly = readOnly;
			Capability = capability;
			CheckSupports();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.CapWrapper`1" /> class.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="capability">The capability.</param>
		/// <param name="getConversionRoutine">The value conversion routine in Get methods.</param>
		/// <param name="setValueProvider">Callback to provide the capability object for set method.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// source
		/// or
		/// getConversionRoutine
		/// or
		/// setValueProvider
		/// </exception>
		public CapWrapper(IDataSource source, CapabilityId capability, Func<object, TValue> getConversionRoutine, Func<TValue, TWOneValue> setValueProvider)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (getConversionRoutine == null)
			{
				throw new ArgumentNullException("getConversionRoutine");
			}
			if (setValueProvider == null)
			{
				throw new ArgumentNullException("setValueProvider");
			}
			_source = source;
			_getConvertRoutine = getConversionRoutine;
			_setOneValueFunc = setValueProvider;
			Capability = capability;
			CheckSupports();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.CapWrapper`1" /> class.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="capability">The capability.</param>
		/// <param name="getConversionRoutine">The value conversion routine in Get methods.</param>
		/// <param name="setValueRoutine">Callback to perform set value.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// source
		/// or
		/// getConversionRoutine
		/// or
		/// setValueRoutine
		/// </exception>
		public CapWrapper(IDataSource source, CapabilityId capability, Func<object, TValue> getConversionRoutine, Func<TValue, ReturnCode> setValueRoutine)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (getConversionRoutine == null)
			{
				throw new ArgumentNullException("getConversionRoutine");
			}
			if (setValueRoutine == null)
			{
				throw new ArgumentNullException("setValueRoutine");
			}
			_source = source;
			_getConvertRoutine = getConversionRoutine;
			_setCustomRoutine = setValueRoutine;
			Capability = capability;
			CheckSupports();
		}

		private void CheckSupports()
		{
			if (_supports.HasValue || !_source.IsOpen)
			{
				return;
			}
			Version protocolVersion = _source.ProtocolVersion;
			Version minimumVersion = ProtocolVersions.GetMinimumVersion(Capability);
			if (protocolVersion >= minimumVersion)
			{
				_supports = _source.Capabilities.QuerySupport(Capability);
				if (_supports.HasValue)
				{
					return;
				}
				using TWCapability capability = new TWCapability(Capability);
				if (_source.DGControl.Capability.Get(capability) == ReturnCode.Success)
				{
					if (IsReadOnly)
					{
						_supports = QuerySupports.Get | QuerySupports.GetDefault | QuerySupports.GetCurrent;
					}
					else
					{
						_supports = QuerySupports.Get | QuerySupports.Set | QuerySupports.GetDefault | QuerySupports.GetCurrent | QuerySupports.Reset | QuerySupports.SetConstraint;
					}
				}
				else
				{
					_supports = QuerySupports.None;
				}
				return;
			}
			_supports = QuerySupports.None;
		}

		private bool Supports(QuerySupports flag)
		{
			return (SupportedActions & flag) == flag;
		}

		/// <summary>
		/// Gets the default value of this capability.
		/// </summary>
		/// <returns></returns>
		public TValue GetDefault()
		{
			if (CanGetDefault)
			{
				return _getConvertRoutine(_source.Capabilities.GetDefault(Capability));
			}
			return default;
		}

		/// <summary>
		/// Gets the current value of this capability.
		/// </summary>
		/// <returns></returns>
		public TValue GetCurrent()
		{
			if (CanGetCurrent)
			{
				return _getConvertRoutine(_source.Capabilities.GetCurrent(Capability));
			}
			return default;
		}

		/// <summary>
		/// Gets all the possible values of this capability without expanding.
		/// This may be required to work with large range values that cannot be safely enumerated
		/// with <see cref="M:NTwain.CapWrapper`1.GetValues" />.
		/// </summary>
		/// <returns></returns>
		public CapabilityReader GetValuesRaw()
		{
			return _source.Capabilities.GetValuesRaw(Capability);
		}

		/// <summary>
		/// Converts the object values into typed values using the conversion routine
		/// for this capability.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public IEnumerable<TValue> ConvertValues(IEnumerable<object> values)
		{
			return values.Select((object o) => _getConvertRoutine(o));
		}

		/// <summary>
		/// Gets all the possible values of this capability.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TValue> GetValues()
		{
			return from o in _source.Capabilities.GetValues(Capability)
				select _getConvertRoutine(o);
		}

		/// <summary>
		/// [Experimental] Gets the label value of this capability.
		/// </summary>
		/// <returns></returns>
		public string GetLabel()
		{
			object obj = null;
			using (TWCapability capability = new TWCapability(Capability))
			{
				if (_source.DGControl.Capability.GetLabel(capability) == ReturnCode.Success)
				{
					CapabilityReader capabilityReader = CapabilityReader.ReadValue(capability);
					if (capabilityReader.ContainerType == ContainerType.OneValue)
					{
						obj = capabilityReader.OneValue;
					}
				}
			}
			return obj?.ToString();
		}

		/// <summary>
		/// [Experimental] Gets the help value of this capability.
		/// </summary>
		/// <returns></returns>
		public string GetHelp()
		{
			object obj = null;
			using (TWCapability capability = new TWCapability(Capability))
			{
				if (_source.DGControl.Capability.GetHelp(capability) == ReturnCode.Success)
				{
					CapabilityReader capabilityReader = CapabilityReader.ReadValue(capability);
					if (capabilityReader.ContainerType == ContainerType.OneValue)
					{
						obj = capabilityReader.OneValue;
					}
				}
			}
			return obj?.ToString();
		}

		/// <summary>
		/// [Experimental] Gets the display names for possible values of this capability.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetLabelEnum()
		{
			new List<object>();
			using (TWCapability capability = new TWCapability(Capability))
			{
				if (_source.DGControl.Capability.GetLabelEnum(capability) == ReturnCode.Success)
				{
					return from o in CapabilityReader.ReadValue(capability).EnumerateCapValues()
						select o.ToString();
				}
			}
			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// Resets the current value to power-on default.
		/// </summary>
		/// <returns></returns>
		public ReturnCode Reset()
		{
			return _source.Capabilities.Reset(Capability);
		}

		/// <summary>
		/// Simplified version that sets the current value of this capability.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="T:System.InvalidOperationException">Simple Set() is not defined for this capability.</exception>
		public ReturnCode SetValue(TValue value)
		{
			ReturnCode result = ReturnCode.Failure;
			if (_setCustomRoutine != null)
			{
				result = _setCustomRoutine(value);
			}
			else
			{
				if (_setOneValueFunc == null)
				{
					throw new InvalidOperationException("Simple Set() is not defined for this capability.");
				}
				using TWCapability capability = new TWCapability(Capability, _setOneValueFunc(value));
				result = _source.DGControl.Capability.Set(capability);
			}
			return result;
		}

		/// <summary>
		/// A version of Set that uses an array.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public ReturnCode SetValue(TWArray value)
		{
			ReturnCode returnCode = ReturnCode.Failure;
			using TWCapability capability = new TWCapability(Capability, value);
			return _source.DGControl.Capability.Set(capability);
		}

		/// <summary>
		/// A version of Set that uses an enumeration.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public ReturnCode SetValue(TWEnumeration value)
		{
			ReturnCode returnCode = ReturnCode.Failure;
			using TWCapability capability = new TWCapability(Capability, value);
			return _source.DGControl.Capability.Set(capability);
		}

		/// <summary>
		/// Sets the constraint value of this capability.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public ReturnCode SetConstraint(TWOneValue value)
		{
			ReturnCode returnCode = ReturnCode.Failure;
			using TWCapability constraint = new TWCapability(Capability, value);
			return _source.DGControl.Capability.SetConstraint(constraint);
		}

		/// <summary>
		/// Sets the constraint value of this capability.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public ReturnCode SetConstraint(TWEnumeration value)
		{
			ReturnCode returnCode = ReturnCode.Failure;
			using TWCapability constraint = new TWCapability(Capability, value);
			return _source.DGControl.Capability.SetConstraint(constraint);
		}

		/// <summary>
		/// Sets the constraint value of this capability.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public ReturnCode SetConstraint(TWRange value)
		{
			ReturnCode returnCode = ReturnCode.Failure;
			using TWCapability constraint = new TWCapability(Capability, value);
			return _source.DGControl.Capability.SetConstraint(constraint);
		}
	}
	/// <summary>
	/// Represents a TWAIN data source.
	/// </summary>
	public class DataSource : IDataSource, ITripletControl
	{
		private ITwainSessionInternal _session;

		private CapabilitiesExt _caps;

		internal TWIdentity Identity { get; private set; }

		/// <summary>
		/// Gets the id of the source.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		public int Id => Identity.Id;

		/// <summary>
		/// Gets the source's product name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public string Name => Identity.ProductName;

		/// <summary>
		/// Gets the source's manufacturer name.
		/// </summary>
		/// <value>
		/// The manufacturer.
		/// </value>
		public string Manufacturer => Identity.Manufacturer;

		/// <summary>
		/// Gets the source's product family.
		/// </summary>
		/// <value>
		/// The product family.
		/// </value>
		public string ProductFamily => Identity.ProductFamily;

		/// <summary>
		/// Gets the source's version information.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
		public TWVersion Version => Identity.Version;

		/// <summary>
		/// Gets the supported data group.
		/// </summary>
		/// <value>
		/// The data group.
		/// </value>
		public DataGroups DataGroup => Identity.DataGroup;

		/// <summary>
		/// Gets the supported TWAIN protocol version.
		/// </summary>
		/// <value>
		/// The protocol version.
		/// </value>
		public Version ProtocolVersion { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this data source has been opened.
		/// </summary>
		/// <value>
		///   <c>true</c> if this data source is open; otherwise, <c>false</c>.
		/// </value>
		public bool IsOpen
		{
			get
			{
				if (_session.IsSourceOpen)
				{
					return _session.CurrentSource == this;
				}
				return false;
			}
		}

		/// <summary>
		/// Gets or sets the current settings (CustomDSData) of this source if supported.
		/// </summary>
		/// <value>
		/// The source settings.
		/// </value>
		public byte[] Settings
		{
			get
			{
				byte[] array = null;
				if (DGControl.CustomDSData.Get(out var customData) == ReturnCode.Success && customData.InfoLength != 0)
				{
					try
					{
						array = new byte[customData.InfoLength];
						Marshal.Copy(PlatformInfo.Current.MemoryManager.Lock(customData.hData), array, 0, (int)customData.InfoLength);
					}
					finally
					{
						PlatformInfo.Current.MemoryManager.Unlock(customData.hData);
						PlatformInfo.Current.MemoryManager.Free(customData.hData);
					}
				}
				return array;
			}
			set
			{
				if (value == null || value.Length == 0)
				{
					return;
				}
				TWCustomDSData tWCustomDSData = new TWCustomDSData
				{
					InfoLength = (uint)value.Length
				};
				try
				{
					tWCustomDSData.hData = PlatformInfo.Current.MemoryManager.Allocate(tWCustomDSData.InfoLength);
					IntPtr destination = PlatformInfo.Current.MemoryManager.Lock(tWCustomDSData.hData);
					Marshal.Copy(value, 0, destination, value.Length);
					DGControl.CustomDSData.Set(tWCustomDSData);
				}
				finally
				{
					if (tWCustomDSData.hData != IntPtr.Zero)
					{
						PlatformInfo.Current.MemoryManager.Unlock(tWCustomDSData.hData);
						PlatformInfo.Current.MemoryManager.Free(tWCustomDSData.hData);
					}
				}
			}
		}

		/// <summary>
		/// Gets the capabilities for this data source.
		/// </summary>
		/// <value>
		/// The capabilities.
		/// </value>
		public ICapabilities Capabilities => _caps ?? (_caps = new CapabilitiesExt(this));

		/// <summary>
		/// Gets the triplet operations defined for control data group.
		/// </summary>
		public DGControl DGControl => _session.DGControl;

		/// <summary>
		/// Gets the triplet operations defined for image data group.
		/// </summary>
		public DGImage DGImage => _session.DGImage;

		/// <summary>
		/// Gets the direct triplet operation entry for custom values.
		/// </summary>
		public DGCustom DGCustom => _session.DGCustom;

		internal DGAudio DGAudio => _session.DGAudio;

		internal DataSource(ITwainSessionInternal session, TWIdentity sourceId)
		{
			_session = session;
			Identity = sourceId;
			ProtocolVersion = new Version(sourceId.ProtocolMajor, sourceId.ProtocolMinor);
		}

		/// <summary>
		/// Opens the source for capability negotiation.
		/// </summary>
		/// <returns></returns>
		public ReturnCode Open()
		{
			ReturnCode rc = ReturnCode.Failure;
			_session.MessageLoopHook.Invoke(() =>
			{
				PlatformInfo.Current.Log.Debug("Thread {0}: OpenSource.", Thread.CurrentThread.ManagedThreadId);
				rc = _session.DGControl.Identity.OpenDS(this);
				_session.UpdateCallback();
			});
			return rc;
		}

		/// <summary>
		/// Closes the source.
		/// </summary>
		/// <returns></returns>
		public ReturnCode Close()
		{
			ReturnCode rc = ReturnCode.Failure;
			_session.MessageLoopHook.Invoke(() =>
			{
				PlatformInfo.Current.Log.Debug("Thread {0}: CloseSource.", Thread.CurrentThread.ManagedThreadId);
				rc = _session.DGControl.Identity.CloseDS();
				_session.UpdateCallback();
			});
			return rc;
		}

		/// <summary>
		/// Enables the source to start transferring.
		/// </summary>
		/// <param name="mode">The mode.</param>
		/// <param name="modal">if set to <c>true</c> any driver UI will display as modal.</param>
		/// <param name="windowHandle">The window handle if modal.</param>
		/// <returns></returns>
		public ReturnCode Enable(SourceEnableMode mode, bool modal, IntPtr windowHandle)
		{
			return _session.EnableSource(mode, modal, windowHandle);
		}

		/// <summary>
		/// Gets the source status. Only call this at state 4 or higher.
		/// </summary>
		/// <returns></returns>
		public TWStatus GetStatus()
		{
			_session.DGControl.Status.GetSource(out var status);
			return status;
		}

		/// <summary>
		/// Gets the source status string. Only call this at state 4 or higher.
		/// </summary>
		/// <param name="status">Status from previous calls.</param>
		/// <returns></returns>
		public TWStatusUtf8 GetStatusUtf8(TWStatus status)
		{
			_session.DGControl.StatusUtf8.GetSource(status, out var utf8status);
			return utf8status;
		}

		/// <summary>
		/// [Experimental] Gets the cameras supported by the source.
		/// </summary>
		/// <returns></returns>
		public IList<string> GetCameras()
		{
			TWFileSystem tWFileSystem = new TWFileSystem();
			List<string> list = new List<string>();
			for (ReturnCode returnCode = DGControl.FileSystem.GetFirstFile(tWFileSystem); returnCode == ReturnCode.Success; returnCode = DGControl.FileSystem.GetNextFile(tWFileSystem))
			{
				FileType fileType = tWFileSystem.FileType;
				if ((uint)fileType <= 3u)
				{
					list.Add(tWFileSystem.OutputName);
				}
			}
			return list;
		}

		/// <summary>
		/// [Experimental] Sets the target camera for cap negotiation that can be set per camera.
		/// </summary>
		/// <param name="cameraName"></param>
		/// <returns></returns>
		public ReturnCode SetCamera(string cameraName)
		{
			TWFileSystem tWFileSystem = new TWFileSystem();
			tWFileSystem.InputName = cameraName;
			return DGControl.FileSystem.ChangeDirectory(tWFileSystem);
		}
	}
	/// <summary>
	/// Contains event data after whatever data from the source has been transferred.
	/// </summary>
	public class DataTransferredEventArgs : EventArgs
	{
		private TWImageInfo _imgInfo;

		/// <summary>
		/// Gets the type of the transfer mode.
		/// </summary>
		/// <value>
		/// The type of the transfer.
		/// </value>
		public XferMech TransferType { get; private set; }

		/// <summary>
		/// Gets the raw pointer to the complete data if <see cref="P:NTwain.DataTransferredEventArgs.TransferType" /> is <see cref="F:NTwain.Data.XferMech.Native" />.
		/// The data will be freed once the event handler ends
		/// so consumers must complete whatever processing before then.
		/// For image type this data is DIB (Windows) or TIFF (Linux).
		/// This pointer is already locked for the duration of this event.
		/// </summary>
		/// <value>The data pointer.</value>
		public IntPtr NativeData { get; private set; }

		/// <summary>
		/// Gets the file path to the complete data if the transfer was file or memory-file.
		/// This is only available if <see cref="P:NTwain.DataTransferredEventArgs.TransferType" /> is <see cref="F:NTwain.Data.XferMech.File" />.
		/// </summary>
		/// <value>
		/// The file path.
		/// </value>
		public string FileDataPath { get; private set; }

		/// <summary>
		/// Gets the file format if applicable.
		/// This is only available if <see cref="P:NTwain.DataTransferredEventArgs.TransferType" /> is <see cref="F:NTwain.Data.XferMech.File" />.
		/// </summary>
		/// <value>
		/// The file format.
		/// </value>
		public FileFormat ImageFileFormat { get; private set; }

		/// <summary>
		/// Gets the info object if the transfer was memory. 
		/// This is only available if <see cref="P:NTwain.DataTransferredEventArgs.TransferType" /> is <see cref="F:NTwain.Data.XferMech.Memory" />.
		/// </summary>
		/// <value>
		/// The memory information.
		/// </value>
		public TWImageMemXfer MemoryInfo { get; private set; }

		/// <summary>
		/// Gets the raw memory data if the transfer was memory.
		/// Consumer application will need to do the parsing based on the values
		/// from <see cref="P:NTwain.DataTransferredEventArgs.ImageInfo" />.
		/// </summary>
		/// <value>
		/// The memory data.
		/// </value>
		public byte[] MemoryData { get; private set; }

		/// <summary>
		/// Gets the final image information if applicable.
		/// </summary>
		/// <value>
		/// The final image information.
		/// </value>
		public TWImageInfo ImageInfo
		{
			get
			{
				if (_imgInfo == null && DataSource.DGImage.ImageInfo.Get(out _imgInfo) != ReturnCode.Success)
				{
					_imgInfo = null;
				}
				return _imgInfo;
			}
		}

		/// <summary>
		/// Gets the data source.
		/// </summary>
		/// <value>
		/// The data source.
		/// </value>
		public DataSource DataSource { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.DataTransferredEventArgs" /> class.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="nativeData">The native data.</param>
		public DataTransferredEventArgs(DataSource source, IntPtr nativeData)
		{
			TransferType = XferMech.Native;
			DataSource = source;
			NativeData = nativeData;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.DataTransferredEventArgs" /> class.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="fileDataPath">The file data path.</param>
		/// <param name="imageFileFormat">The image file format.</param>
		public DataTransferredEventArgs(DataSource source, string fileDataPath, FileFormat imageFileFormat)
		{
			TransferType = XferMech.File;
			DataSource = source;
			FileDataPath = fileDataPath;
			ImageFileFormat = imageFileFormat;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.DataTransferredEventArgs" /> class.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="memoryInfo">The memory information.</param>
		/// <param name="memoryData">The memory data.</param>
		public DataTransferredEventArgs(DataSource source, TWImageMemXfer memoryInfo, byte[] memoryData)
		{
			TransferType = XferMech.Memory;
			DataSource = source;
			MemoryInfo = memoryInfo;
			MemoryData = memoryData;
		}

		/// <summary>
		/// Gets the extended image information if applicable.
		/// </summary>
		/// <param name="infoIds">The information ids.</param>
		/// <returns></returns>
		/// <exception cref="T:System.InvalidOperationException"></exception>
		public IEnumerable<TWInfo> GetExtImageInfo(params ExtendedImageInfo[] infoIds)
		{
			if (infoIds != null && infoIds.Length != 0)
			{
				TWExtImageInfo tWExtImageInfo = new TWExtImageInfo
				{
					NumInfos = (uint)infoIds.Length
				};
				if (infoIds.Length > tWExtImageInfo.Info.Length)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Info ID array exceeded maximum length of {0}.", tWExtImageInfo.Info.Length));
				}
				for (int i = 0; i < infoIds.Length; i++)
				{
					tWExtImageInfo.Info[i].InfoID = infoIds[i];
				}
				if (DataSource.DGImage.ExtImageInfo.Get(tWExtImageInfo) == ReturnCode.Success)
				{
					return tWExtImageInfo.Info.Where((TWInfo it) => it.InfoID != ExtendedImageInfo.Invalid);
				}
			}
			return Enumerable.Empty<TWInfo>();
		}

		/// <summary>
		/// Gets the image stream from the <see cref="P:NTwain.DataTransferredEventArgs.NativeData" /> if it's an image.
		/// </summary>
		/// <returns></returns>
		public Stream GetNativeImageStream()
		{
			Stream result = null;
			if (NativeData != IntPtr.Zero)
			{
				if (ImageTools.IsDib(NativeData))
				{
					result = ImageTools.GetBitmapStream(NativeData);
				}
				else if (ImageTools.IsTiff(NativeData))
				{
					result = ImageTools.GetTiffStream(NativeData);
				}
			}
			return result;
		}
	}
	/// <summary>
	/// Contains event data for a TWAIN source hardware event.
	/// </summary>
	public class DeviceEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the detailed device event.
		/// </summary>
		/// <value>The device event.</value>
		public TWDeviceEvent DeviceEvent { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.DeviceEventArgs" /> class.
		/// </summary>
		/// <param name="deviceEvent">The device event.</param>
		internal DeviceEventArgs(TWDeviceEvent deviceEvent)
		{
			DeviceEvent = deviceEvent;
		}
	}
	/// <summary>
	/// Exposes capabilities of a data source as properties.
	/// </summary>
	public interface ICapabilities
	{
		/// <summary>
		/// Gets the property to work with audio <see cref="T:NTwain.Data.XferMech" /> for the current source.
		/// </summary>
		/// <value>
		/// The audio xfer mech.
		/// </value>
		ICapWrapper<XferMech> ACapXferMech { get; }

		/// <summary>
		/// Gets the property to work with alarms for the current source.
		/// </summary>
		/// <value>
		/// The alarms.
		/// </value>
		ICapWrapper<AlarmType> CapAlarms { get; }

		/// <summary>
		/// Gets the property to work with alarm volume for the current source.
		/// </summary>
		/// <value>
		/// The alarm volume.
		/// </value>
		ICapWrapper<int> CapAlarmVolume { get; }

		/// <summary>
		/// Gets the property to work with the name or other identifying information about the 
		/// Author of the image. It may include a copyright string.
		/// </summary>
		/// <value>
		/// The author string.
		/// </value>
		ICapWrapper<string> CapAuthor { get; }

		/// <summary>
		/// Gets the property to work with auto feed page flag for the current source.
		/// </summary>
		/// <value>
		/// The auto feed flag.
		/// </value>
		ICapWrapper<BoolType> CapAutoFeed { get; }

		/// <summary>
		/// Gets the property to work with auto capture count for the current source.
		/// </summary>
		/// <value>
		/// The auto capture count.
		/// </value>
		ICapWrapper<int> CapAutomaticCapture { get; }

		/// <summary>
		/// Gets the property to work with auto-sense medium (paper source) flag.
		/// </summary>
		/// <value>
		/// The auto-sense medium flag.
		/// </value>
		ICapWrapper<BoolType> CapAutomaticSenseMedium { get; }

		/// <summary>
		/// Gets the property to work with auto scan page flag for the current source.
		/// </summary>
		/// <value>
		/// The auto scan flag.
		/// </value>
		ICapWrapper<BoolType> CapAutoScan { get; }

		/// <summary>
		/// Gets the property to see the remaining battery power for the device.
		/// </summary>
		/// <value>
		/// The battery minutes.
		/// </value>
		IReadOnlyCapWrapper<int> CapBatteryMinutes { get; }

		/// <summary>
		/// Gets the property to see the remaining battery percentage for the device.
		/// </summary>
		/// <value>
		/// The battery percentage.
		/// </value>
		IReadOnlyCapWrapper<int> CapBatteryPercentage { get; }

		/// <summary>
		/// Gets the property to work with camera enabled flag.
		/// </summary>
		/// <value>
		/// The camera enabled flag.
		/// </value>
		ICapWrapper<BoolType> CapCameraEnabled { get; }

		/// <summary>
		/// Gets the property to work with camera order for the current source.
		/// </summary>
		/// <value>
		/// The camera order setting.
		/// </value>
		ICapWrapper<PixelType> CapCameraOrder { get; }

		/// <summary>
		/// Gets the property to see whether device supports camera preview UI flag.
		/// </summary>
		/// <value>
		/// The camera preview UI flag.
		/// </value>
		IReadOnlyCapWrapper<BoolType> CapCameraPreviewUI { get; }

		/// <summary>
		/// Gets the property to work with camera side for the current source.
		/// </summary>
		/// <value>
		/// The camera side.
		/// </value>
		ICapWrapper<CameraSide> CapCameraSide { get; }

		/// <summary>
		/// Gets the property to work with the general note about the acquired image.
		/// </summary>
		/// <value>
		/// The general note string.
		/// </value>
		ICapWrapper<string> CapCaption { get; }

		/// <summary>
		/// Gets the property to work with the clear buffers option for the current source.
		/// </summary>
		/// <value>
		/// The clear buffers option.
		/// </value>
		ICapWrapper<ClearBuffer> CapClearBuffers { get; }

		/// <summary>
		/// Gets the property to work with clear page flag for the current source.
		/// </summary>
		/// <value>
		/// The clear page flag.
		/// </value>
		ICapWrapper<BoolType> CapClearPage { get; }

		/// <summary>
		/// Gets the property to see whether device supports custom data triplets.
		/// </summary>
		/// <value>
		/// The custom data flag.
		/// </value>
		IReadOnlyCapWrapper<BoolType> CapCustomDSData { get; }

		/// <summary>
		/// Gets the property for device interface guid.
		/// </summary>
		/// <value>
		/// The device interface guid.
		/// </value>
		IReadOnlyCapWrapper<string> CapCustomInterfaceGuid { get; }

		/// <summary>
		/// Gets the property to work with the reported device events for the current source.
		/// </summary>
		/// <value>
		/// The reported device events.
		/// </value>
		ICapWrapper<NTwain.Data.DeviceEvent> CapDeviceEvent { get; }

		/// <summary>
		/// Gets the property to work with devince online flag for the current source.
		/// </summary>
		/// <value>
		/// The devince online flag.
		/// </value>
		IReadOnlyCapWrapper<BoolType> CapDeviceOnline { get; }

		/// <summary>
		/// Gets the property to work with the device's time and date.
		/// </summary>
		/// <value>
		/// The device time and date.
		/// </value>
		ICapWrapper<string> CapDeviceTimeDate { get; }

		/// <summary>
		/// Gets the property to work with double feed detection option for the current source.
		/// </summary>
		/// <value>
		/// The double feed detection option.
		/// </value>
		ICapWrapper<DoubleFeedDetection> CapDoubleFeedDetection { get; }

		/// <summary>
		/// Gets the property to work with double feed detection length for the current source.
		/// </summary>
		/// <value>
		/// The double feed detection length.
		/// </value>
		ICapWrapper<TWFix32> CapDoubleFeedDetectionLength { get; }

		/// <summary>
		/// Gets the property to work with double feed detection response for the current source.
		/// </summary>
		/// <value>
		/// The double feed detection response.
		/// </value>
		ICapWrapper<DoubleFeedDetectionResponse> CapDoubleFeedDetectionResponse { get; }

		/// <summary>
		/// Gets the property to work with double feed detection sensitivity for the current source.
		/// </summary>
		/// <value>
		/// The double feed detection sensitivity.
		/// </value>
		ICapWrapper<DoubleFeedDetectionSensitivity> CapDoubleFeedDetectionSensitivity { get; }

		/// <summary>
		/// Gets the property to see what's the duplex mode for the current source.
		/// </summary>
		/// <value>
		/// The duplex mode.
		/// </value>
		IReadOnlyCapWrapper<Duplex> CapDuplex { get; }

		/// <summary>
		/// Gets the property to work with duplex enabled flag for the current source.
		/// </summary>
		/// <value>
		/// The duplex enabled flag.
		/// </value>
		ICapWrapper<BoolType> CapDuplexEnabled { get; }

		/// <summary>
		/// Gets the property to see whether device supports UI only flag (no transfer).
		/// </summary>
		/// <value>
		/// The UI only flag.
		/// </value>
		IReadOnlyCapWrapper<BoolType> CapEnableDSUIOnly { get; }

		/// <summary>
		/// Gets the property to work with endorser for the current source.
		/// </summary>
		/// <value>
		/// The endorser option.
		/// </value>
		ICapWrapper<uint> CapEndorser { get; }

		/// <summary>
		/// Gets the extended caps for the current source.
		/// </summary>
		/// <value>
		/// The extended caps.
		/// </value>
		ICapWrapper<CapabilityId> CapExtendedCaps { get; }

		/// <summary>
		/// Gets the property to work with feeder alignment for the current source.
		/// </summary>
		/// <value>
		/// The feeder alignment.
		/// </value>
		ICapWrapper<FeederAlignment> CapFeederAlignment { get; }

		/// <summary>
		/// Gets the property to work with feeder enabled flag for the current source.
		/// </summary>
		/// <value>
		/// The feeder enabled flag.
		/// </value>
		ICapWrapper<BoolType> CapFeederEnabled { get; }

		/// <summary>
		/// Gets the property to work with feeder loaded flag for the current source.
		/// </summary>
		/// <value>
		/// The feeder loaded flag.
		/// </value>
		IReadOnlyCapWrapper<BoolType> CapFeederLoaded { get; }

		/// <summary>
		/// Gets the property to work with feeder order for the current source.
		/// </summary>
		/// <value>
		/// The feeder order.
		/// </value>
		ICapWrapper<FeederOrder> CapFeederOrder { get; }

		/// <summary>
		/// Gets the property to work with feeder pocket for the current source.
		/// </summary>
		/// <value>
		/// The feeder pocket setting.
		/// </value>
		ICapWrapper<FeederPocket> CapFeederPocket { get; }

		/// <summary>
		/// Gets the property to work with feeder prep flag.
		/// </summary>
		/// <value>
		/// The feeder prep flag.
		/// </value>
		ICapWrapper<BoolType> CapFeederPrep { get; }

		/// <summary>
		/// Gets the property to work with feed page flag for the current source.
		/// </summary>
		/// <value>
		/// The feed page flag.
		/// </value>
		ICapWrapper<BoolType> CapFeedPage { get; }

		/// <summary>
		/// Gets the property to work with indicators flag for the current source.
		/// </summary>
		/// <value>
		/// The indicators flag.
		/// </value>
		ICapWrapper<BoolType> CapIndicators { get; }

		/// <summary>
		/// Gets the property to work with diplayed indicators for the current source.
		/// </summary>
		/// <value>
		/// The diplayed indicators.
		/// </value>
		ICapWrapper<IndicatorsMode> CapIndicatorsMode { get; }

		/// <summary>
		/// Gets the property to work with job control option for the current source.
		/// </summary>
		/// <value>
		/// The job control option.
		/// </value>
		ICapWrapper<JobControl> CapJobControl { get; }

		/// <summary>
		/// Gets the property to work with string data language for the current source.
		/// </summary>
		/// <value>
		/// The language.
		/// </value>
		ICapWrapper<Language> CapLanguage { get; }

		/// <summary>
		/// Gets the property to work with the max buffered pages for the current source.
		/// </summary>
		/// <value>
		/// The max batch buffered pages.
		/// </value>
		ICapWrapper<uint> CapMaxBatchBuffers { get; }

		/// <summary>
		/// Gets the property to work with check scanning support flag.
		/// </summary>
		/// <value>
		/// The check scanning support flag.
		/// </value>
		ICapWrapper<BoolType> CapMicrEnabled { get; }

		/// <summary>
		/// Gets the property to work with paper sensor flag for the current source.
		/// </summary>
		/// <value>
		/// The paper sensor flag.
		/// </value>
		IReadOnlyCapWrapper<BoolType> CapPaperDetectable { get; }

		/// <summary>
		/// Gets the property to work with paper handling option for the current source.
		/// </summary>
		/// <value>
		/// The paper handling option.
		/// </value>
		ICapWrapper<PaperHandling> CapPaperHandling { get; }

		/// <summary>
		/// Gets the property to work with camera power down time (seconds) for the current source.
		/// </summary>
		/// <value>
		/// The camera power down time.
		/// </value>
		ICapWrapper<int> CapPowerSaveTime { get; }

		/// <summary>
		/// Gets the property to see current device's power supply.
		/// </summary>
		/// <value>
		/// The power supply indicator.
		/// </value>
		IReadOnlyCapWrapper<PowerSupply> CapPowerSupply { get; }

		/// <summary>
		/// Gets the property to work with printer list for the current source.
		/// </summary>
		/// <value>
		/// The printer list.
		/// </value>
		ICapWrapper<Printer> CapPrinter { get; }

		/// <summary>
		/// Gets the property to work with printer character rotation for the current source.
		/// </summary>
		/// <value>
		/// The printer character rotation.
		/// </value>
		ICapWrapper<int> CapPrinterCharRotation { get; }

		/// <summary>
		/// Gets the property to work with printer enabled flag.
		/// </summary>
		/// <value>
		/// The printer enabled flag.
		/// </value>
		ICapWrapper<BoolType> CapPrinterEnabled { get; }

		/// <summary>
		/// Gets the property to work with printer font style for the current source.
		/// </summary>
		/// <value>
		/// The printer font style.
		/// </value>
		ICapWrapper<PrinterFontStyle> CapPrinterFontStyle { get; }

		/// <summary>
		/// Gets the property to work with the starting printer index for the current source.
		/// </summary>
		/// <value>
		/// The printer index.
		/// </value>
		ICapWrapper<int> CapPrinterIndex { get; }

		/// <summary>
		/// Set the character to be used for filling the leading digits before the counter value if the
		/// counter digits are fewer than <see cref="P:NTwain.ICapabilities.CapPrinterIndexNumDigits" />.
		/// </summary>
		/// <value>
		/// The printer leading string.
		/// </value>
		ICapWrapper<string> CapPrinterIndexLeadChar { get; }

		/// <summary>
		/// Gets the property to work with printer index max value for the current source.
		/// </summary>
		/// <value>
		/// The printer index max value.
		/// </value>
		ICapWrapper<uint> CapPrinterIndexMaxValue { get; }

		/// <summary>
		/// Gets the property to work with printer number digits value for the current source.
		/// </summary>
		/// <value>
		/// The printer number digits value.
		/// </value>
		ICapWrapper<int> CapPrinterIndexNumDigits { get; }

		/// <summary>
		/// Gets the property to work with printer index step value for the current source.
		/// </summary>
		/// <value>
		/// The printer index step value.
		/// </value>
		ICapWrapper<int> CapPrinterIndexStep { get; }

		/// <summary>
		/// Gets the property to work with printer index trigger for the current source.
		/// </summary>
		/// <value>
		/// The printer index trigger.
		/// </value>
		ICapWrapper<PrinterIndexTrigger> CapPrinterIndexTrigger { get; }

		/// <summary>
		/// Gets the property to work with printer mode for the current source.
		/// </summary>
		/// <value>
		/// The printer mode.
		/// </value>
		ICapWrapper<PrinterMode> CapPrinterMode { get; }

		/// <summary>
		/// Specifies the string(s) that are to be used in the string component when the current <see cref="P:NTwain.ICapabilities.CapPrinter" />
		/// device is enabled.
		/// </summary>
		/// <value>
		/// The printer string.
		/// </value>
		ICapWrapper<string> CapPrinterString { get; }

		/// <summary>
		/// Gets the next print values.
		/// </summary>
		/// <value>
		/// The next print values.
		/// </value>
		IReadOnlyCapWrapper<string> CapPrinterStringPreview { get; }

		/// <summary>
		/// Specifies the string that shall be used as the current <see cref="P:NTwain.ICapabilities.CapPrinter" /> device’s suffix.
		/// </summary>
		/// <value>
		/// The printer suffix string.
		/// </value>
		ICapWrapper<string> CapPrinterSuffix { get; }

		/// <summary>
		/// Gets the property to work with printer y-offset for the current source.
		/// </summary>
		/// <value>
		/// The printer y-offset.
		/// </value>
		ICapWrapper<TWFix32> CapPrinterVerticalOffset { get; }

		/// <summary>
		/// Gets the property to see whether device supports reacquire flag.
		/// </summary>
		/// <value>
		/// The reacquire flag.
		/// </value>
		IReadOnlyCapWrapper<BoolType> CapReacquireAllowed { get; }

		/// <summary>
		/// Gets the property to work with rewind page flag for the current source.
		/// </summary>
		/// <value>
		/// The rewind page flag.
		/// </value>
		ICapWrapper<BoolType> CapRewindPage { get; }

		/// <summary>
		/// Gets the property to work with segmentation setting for the current source.
		/// </summary>
		/// <value>
		/// The segmentation setting.
		/// </value>
		ICapWrapper<Segmented> CapSegmented { get; }

		/// <summary>
		/// Gets the property for device serial number.
		/// </summary>
		/// <value>
		/// The device serial number.
		/// </value>
		IReadOnlyCapWrapper<string> CapSerialNumber { get; }

		/// <summary>
		/// Gets the supported caps for the current source. This is not supported by all sources.
		/// </summary>
		/// <value>
		/// The supported caps.
		/// </value>
		IReadOnlyCapWrapper<CapabilityId> CapSupportedCaps { get; }

		/// <summary>
		/// Gets the supported caps for unique segments for the current source.
		/// </summary>
		/// <value>
		/// The supported caps for unique segments.
		/// </value>
		IReadOnlyCapWrapper<CapabilityId> CapSupportedCapsSegmentUnique { get; }

		/// <summary>
		/// Gets the supported caps for supported DATs.
		/// </summary>
		/// <value>
		/// The supported DATs.
		/// </value>
		IReadOnlyCapWrapper<uint> CapSupportedDATs { get; }

		/// <summary>
		/// Gets the property to work with thumbnails enabled flag for the current source.
		/// </summary>
		/// <value>
		/// The thumbnails enabled flag.
		/// </value>
		ICapWrapper<BoolType> CapThumbnailsEnabled { get; }

		/// <summary>
		/// Gets the property to work with the time before first capture (milliseconds) for the current source.
		/// </summary>
		/// <value>
		/// The time before first capture.
		/// </value>
		ICapWrapper<int> CapTimeBeforeFirstCapture { get; }

		/// <summary>
		/// Gets the property to work with the time between captures (milliseconds) for the current source.
		/// </summary>
		/// <value>
		/// The time between captures.
		/// </value>
		ICapWrapper<int> CapTimeBetweenCaptures { get; }

		/// <summary>
		/// Gets the property to get the image acquired time and date.
		/// </summary>
		/// <value>
		/// The time and date string.
		/// </value>
		IReadOnlyCapWrapper<string> CapTimeDate { get; }

		/// <summary>
		/// Gets the property to work with UI controllable flag for the current source.
		/// </summary>
		/// <value>
		/// The UI controllable flag.
		/// </value>
		IReadOnlyCapWrapper<BoolType> CapUIControllable { get; }

		/// <summary>
		/// Gets the property to work with xfer count for the current source.
		/// </summary>
		/// <value>
		/// The xfer count.
		/// </value>
		ICapWrapper<int> CapXferCount { get; }

		/// <summary>
		/// Gets the property to work with image auto brightness flag for the current source.
		/// </summary>
		/// <value>
		/// The image auto brightness flag.
		/// </value>
		ICapWrapper<BoolType> ICapAutoBright { get; }

		/// <summary>
		/// Gets the property to work with image blank page behavior for the current source.
		/// </summary>
		/// <value>
		/// The image blank page behavior.
		/// </value>
		ICapWrapper<BlankPage> ICapAutoDiscardBlankPages { get; }

		/// <summary>
		/// Gets the property to work with auto border detection flag for the current source.
		/// </summary>
		/// <value>
		/// The auto border detection flag.
		/// </value>
		ICapWrapper<BoolType> ICapAutomaticBorderDetection { get; }

		/// <summary>
		/// Gets the property to work with image auto color detection flag for the current source.
		/// </summary>
		/// <value>
		/// The image auto color detection flag.
		/// </value>
		ICapWrapper<BoolType> ICapAutomaticColorEnabled { get; }

		/// <summary>
		/// Gets the property to work with image auto non-color pixel type for the current source.
		/// </summary>
		/// <value>
		/// The image auto non-color pixel type.
		/// </value>
		ICapWrapper<PixelType> ICapAutomaticColorNonColorPixelType { get; }

		/// <summary>
		/// Gets the property to work with image auto crop flag for the current source.
		/// </summary>
		/// <value>
		/// The image auto crop flag.
		/// </value>
		IReadOnlyCapWrapper<BoolType> ICapAutomaticCropUsesFrame { get; }

		/// <summary>
		/// Gets the property to work with image auto deskew flag for the current source.
		/// </summary>
		/// <value>
		/// The image auto deskew flag.
		/// </value>
		ICapWrapper<BoolType> ICapAutomaticDeskew { get; }

		/// <summary>
		/// Gets the property to work with image auto length detection flag for the current source.
		/// </summary>
		/// <value>
		/// The image auto length detection flag.
		/// </value>
		ICapWrapper<BoolType> ICapAutomaticLengthDetection { get; }

		/// <summary>
		/// Gets the property to work with image auto rotate flag for the current source.
		/// </summary>
		/// <value>
		/// The image auto rotate flag.
		/// </value>
		ICapWrapper<BoolType> ICapAutomaticRotate { get; }

		/// <summary>
		/// Gets the property to work with image auto size option for the current source.
		/// </summary>
		/// <value>
		/// The image auto size option.
		/// </value>
		ICapWrapper<AutoSize> ICapAutoSize { get; }

		/// <summary>
		/// Gets the property to work with image barcode detection flag for the current source.
		/// </summary>
		/// <value>
		/// The image barcode detection flag.
		/// </value>
		ICapWrapper<BoolType> ICapBarcodeDetectionEnabled { get; }

		/// <summary>
		/// Gets the property to work with image barcode max search retries for the current source.
		/// </summary>
		/// <value>
		/// The image barcode max search retries.
		/// </value>
		ICapWrapper<uint> ICapBarcodeMaxRetries { get; }

		/// <summary>
		/// Gets the property to work with image barcode max search priorities for the current source.
		/// </summary>
		/// <value>
		/// The image barcode max search priorities.
		/// </value>
		ICapWrapper<uint> ICapBarcodeMaxSearchPriorities { get; }

		/// <summary>
		/// Gets the property to work with image barcode search direction for the current source.
		/// </summary>
		/// <value>
		/// The image barcode search direction.
		/// </value>
		ICapWrapper<BarcodeDirection> ICapBarcodeSearchMode { get; }

		/// <summary>
		/// Gets the property to work with image barcode search priority for the current source.
		/// </summary>
		/// <value>
		/// The image barcode search priority.
		/// </value>
		ICapWrapper<BarcodeType> ICapBarcodeSearchPriorities { get; }

		/// <summary>
		/// Gets the property to work with image barcode max search timeout for the current source.
		/// </summary>
		/// <value>
		/// The image barcode max search timeout.
		/// </value>
		ICapWrapper<uint> ICapBarcodeTimeout { get; }

		/// <summary>
		/// Gets the property to work with image bit depth for the current source.
		/// </summary>
		/// <value>
		/// The image bit depth.
		/// </value>
		ICapWrapper<int> ICapBitDepth { get; }

		/// <summary>
		/// Gets the property to work with image bit depth reduction method for the current source.
		/// </summary>
		/// <value>
		/// The image bit depth reduction method.
		/// </value>
		ICapWrapper<BitDepthReduction> ICapBitDepthReduction { get; }

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.BitOrder" /> for the current source.
		/// </summary>
		/// <value>
		/// The image bit order.
		/// </value>
		ICapWrapper<BitOrder> ICapBitOrder { get; }

		/// <summary>
		/// Gets the property to work with image CCITT compression <see cref="T:NTwain.Data.BitOrder" /> for the current source.
		/// </summary>
		/// <value>
		/// The image bit order for CCITT compression.
		/// </value>
		ICapWrapper<BitOrder> ICapBitOrderCodes { get; }

		/// <summary>
		/// Gets the property to work with image brightness for the current source.
		/// </summary>
		/// <value>
		/// The image brightness.
		/// </value>
		ICapWrapper<TWFix32> ICapBrightness { get; }

		/// <summary>
		/// Gets the property to work with image CCITT K factor for the current source.
		/// </summary>
		/// <value>
		/// The image CCITT K factor.
		/// </value>
		ICapWrapper<int> ICapCCITTKFactor { get; }

		/// <summary>
		/// Gets the property to work with image color management flag for the current source.
		/// </summary>
		/// <value>
		/// The image color management flag.
		/// </value>
		ICapWrapper<BoolType> ICapColorManagementEnabled { get; }

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.CompressionType" /> for the current source.
		/// </summary>
		/// <value>
		/// The image compression.
		/// </value>
		ICapWrapper<CompressionType> ICapCompression { get; }

		/// <summary>
		/// Gets the property to work with image contrast for the current source.
		/// </summary>
		/// <value>
		/// The image contrast.
		/// </value>
		ICapWrapper<TWFix32> ICapContrast { get; }

		/// <summary>
		/// Gets the property to work with image square-cell halftone for the current source.
		/// </summary>
		/// <value>
		/// The image square-cell halftone.
		/// </value>
		ICapWrapper<byte> ICapCustHalftone { get; }

		/// <summary>
		/// Gets the property to work with image exposure time (in seconds) for the current source.
		/// </summary>
		/// <value>
		/// The image exposure time.
		/// </value>
		ICapWrapper<TWFix32> ICapExposureTime { get; }

		/// <summary>
		/// Gets the property to work with ext image info flag for the current source.
		/// </summary>
		/// <value>
		/// The ext image info flag.
		/// </value>
		ICapWrapper<BoolType> ICapExtImageInfo { get; }

		/// <summary>
		/// Gets the property to work with feeder type for the current source.
		/// </summary>
		/// <value>
		/// The feeder type.
		/// </value>
		ICapWrapper<FeederType> ICapFeederType { get; }

		/// <summary>
		/// Gets the property to work with image film type for the current source.
		/// </summary>
		/// <value>
		/// The image film type.
		/// </value>
		ICapWrapper<FilmType> ICapFilmType { get; }

		/// <summary>
		/// Gets the property to work with image color filter for the current source.
		/// </summary>
		/// <value>
		/// The image color filter type.
		/// </value>
		ICapWrapper<FilterType> ICapFilter { get; }

		/// <summary>
		/// Gets the property to work with flash option for the current source.
		/// </summary>
		/// <value>
		/// The flash option.
		/// </value>
		ICapWrapper<FlashedUsed> ICapFlashUsed2 { get; }

		/// <summary>
		/// Gets the property to work with image flip-rotation behavior for the current source.
		/// </summary>
		/// <value>
		/// The image flip-rotation behavior.
		/// </value>
		ICapWrapper<FlipRotation> ICapFlipRotation { get; }

		/// <summary>
		/// Gets the property to work with the list of frames the source will acquire on each page.
		/// </summary>
		/// <value>
		/// The capture frames.
		/// </value>
		ICapWrapper<TWFrame> ICapFrames { get; }

		/// <summary>
		/// Gets the property to work with image gamma value for the current source.
		/// </summary>
		/// <value>
		/// The image gamma.
		/// </value>
		ICapWrapper<TWFix32> ICapGamma { get; }

		/// <summary>
		/// Gets the property to work with image halftone patterns for the current source.
		/// </summary>
		/// <value>
		/// The image halftone patterns.
		/// </value>
		ICapWrapper<string> ICapHalftones { get; }

		/// <summary>
		/// Gets the property to work with image highlight value for the current source.
		/// </summary>
		/// <value>
		/// The image highlight.
		/// </value>
		ICapWrapper<TWFix32> ICapHighlight { get; }

		/// <summary>
		/// Gets the property to work with image icc profile for the current source.
		/// </summary>
		/// <value>
		/// The image icc profile.
		/// </value>
		ICapWrapper<NTwain.Data.IccProfile> ICapICCProfile { get; }

		/// <summary>
		/// Gets or sets the image indices that will be delivered during the standard image transfer done in
		/// States 6 and 7.
		/// </summary>
		/// <value>
		/// The image indicies.
		/// </value>
		ICapWrapper<uint> ICapImageDataSet { get; }

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.FileFormat" /> for the current source.
		/// </summary>
		/// <value>
		/// The image file format.
		/// </value>
		ICapWrapper<FileFormat> ICapImageFileFormat { get; }

		/// <summary>
		/// Gets the property to work with image enhancement filter for the current source.
		/// </summary>
		/// <value>
		/// The image enhancement filter.
		/// </value>
		ICapWrapper<ImageFilter> ICapImageFilter { get; }

		/// <summary>
		/// Gets the property to work with image merge option for the current source.
		/// </summary>
		/// <value>
		/// The image merge option.
		/// </value>
		ICapWrapper<ImageMerge> ICapImageMerge { get; }

		/// <summary>
		/// Gets the property to work with image merge height threshold for the current source.
		/// </summary>
		/// <value>
		/// The image merge height threshold.
		/// </value>
		ICapWrapper<TWFix32> ICapImageMergeHeightThreshold { get; }

		/// <summary>
		/// Gets the property to work with image jpeg compression <see cref="T:NTwain.Data.PixelType" /> for the current source.
		/// </summary>
		/// <value>
		/// The image pixel type for jpeg compression.
		/// </value>
		ICapWrapper<PixelType> ICapJpegPixelType { get; }

		/// <summary>
		/// Gets the property to work with image jpeg quality for the current source.
		/// </summary>
		/// <value>
		/// The image jpeg quality.
		/// </value>
		ICapWrapper<JpegQuality> ICapJpegQuality { get; }

		/// <summary>
		/// Gets the property to work with image jpeg sub sampling for the current source.
		/// </summary>
		/// <value>
		/// The image jpeg sub sampling.
		/// </value>
		ICapWrapper<JpegSubsampling> ICapJpegSubsampling { get; }

		/// <summary>
		/// Gets the property to work with image lamp state flag for the current source.
		/// </summary>
		/// <value>
		/// The image lamp state flag.
		/// </value>
		ICapWrapper<BoolType> ICapLampState { get; }

		/// <summary>
		/// Gets the property to work with image light path for the current source.
		/// </summary>
		/// <value>
		/// The image light path.
		/// </value>
		ICapWrapper<LightPath> ICapLightPath { get; }

		/// <summary>
		/// Gets the property to work with image light source for the current source.
		/// </summary>
		/// <value>
		/// The image light source.
		/// </value>
		ICapWrapper<LightSource> ICapLightSource { get; }

		/// <summary>
		/// Gets the property to work with image max frames for the current source.
		/// </summary>
		/// <value>
		/// The image max frames.
		/// </value>
		ICapWrapper<int> ICapMaxFrames { get; }

		/// <summary>
		/// Gets the property to work with image minimum height for the current source.
		/// </summary>
		/// <value>
		/// The image minimumm height.
		/// </value>
		IReadOnlyCapWrapper<TWFix32> ICapMinimumHeight { get; }

		/// <summary>
		/// Gets the property to work with image minimum width for the current source.
		/// </summary>
		/// <value>
		/// The image minimumm width.
		/// </value>
		IReadOnlyCapWrapper<TWFix32> ICapMinimumWidth { get; }

		/// <summary>
		/// Gets the property to work with image mirror option for the current source.
		/// </summary>
		/// <value>
		/// The image mirror option.
		/// </value>
		ICapWrapper<Mirror> ICapMirror { get; }

		/// <summary>
		/// Gets the property to work with image noise filter for the current source.
		/// </summary>
		/// <value>
		/// The image noise filter.
		/// </value>
		ICapWrapper<NoiseFilter> ICapNoiseFilter { get; }

		/// <summary>
		/// Gets the property to work with image orientation for the current source.
		/// </summary>
		/// <value>
		/// The image orientation.
		/// </value>
		ICapWrapper<OrientationType> ICapOrientation { get; }

		/// <summary>
		/// Gets the property to work with image overscan option for the current source.
		/// </summary>
		/// <value>
		/// The image overscan option.
		/// </value>
		ICapWrapper<OverScan> ICapOverScan { get; }

		/// <summary>
		/// Gets the property to work with image patch code detection flag for the current source.
		/// </summary>
		/// <value>
		/// The image patch code detection flag.
		/// </value>
		ICapWrapper<BoolType> ICapPatchCodeDetectionEnabled { get; }

		/// <summary>
		/// Gets the property to work with image patch code max search retries for the current source.
		/// </summary>
		/// <value>
		/// The image patch code max search retries.
		/// </value>
		ICapWrapper<uint> ICapPatchCodeMaxRetries { get; }

		/// <summary>
		/// Gets the property to work with image patch code max search priorities for the current source.
		/// </summary>
		/// <value>
		/// The image patch code max search priorities.
		/// </value>
		ICapWrapper<uint> ICapPatchCodeMaxSearchPriorities { get; }

		/// <summary>
		/// Gets the property to work with image patch code search direction for the current source.
		/// </summary>
		/// <value>
		/// The image patch code search direction.
		/// </value>
		ICapWrapper<BarcodeDirection> ICapPatchCodeSearchMode { get; }

		/// <summary>
		/// Gets the property to work with image patch code search priority for the current source.
		/// </summary>
		/// <value>
		/// The image patch code search priority.
		/// </value>
		ICapWrapper<PatchCode> ICapPatchCodeSearchPriorities { get; }

		/// <summary>
		/// Gets the property to work with image patch code max search timeout for the current source.
		/// </summary>
		/// <value>
		/// The image patch code max search timeout.
		/// </value>
		ICapWrapper<uint> ICapPatchCodeTimeout { get; }

		/// <summary>
		/// Gets the property to work with image physical height for the current source.
		/// </summary>
		/// <value>
		/// The image physical height.
		/// </value>
		IReadOnlyCapWrapper<TWFix32> ICapPhysicalHeight { get; }

		/// <summary>
		/// Gets the property to work with image physical width for the current source.
		/// </summary>
		/// <value>
		/// The image physical width.
		/// </value>
		IReadOnlyCapWrapper<TWFix32> ICapPhysicalWidth { get; }

		/// <summary>
		/// Gets the property to work with image pixel flavor for the current source.
		/// </summary>
		/// <value>
		/// The image pixel flavor.
		/// </value>
		ICapWrapper<PixelFlavor> ICapPixelFlavor { get; }

		/// <summary>
		/// Gets the property to work with image CCITT compression <see cref="T:NTwain.Data.PixelFlavor" /> for the current source.
		/// </summary>
		/// <value>
		/// The image pixel flavor for CCITT compression.
		/// </value>
		ICapWrapper<PixelFlavor> ICapPixelFlavorCodes { get; }

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.PixelType" /> for the current source.
		/// </summary>
		/// <value>
		/// The image pixel type.
		/// </value>
		ICapWrapper<PixelType> ICapPixelType { get; }

		/// <summary>
		/// Gets the property to work with image color format for the current source.
		/// </summary>
		/// <value>
		/// The image color format.
		/// </value>
		ICapWrapper<PlanarChunky> ICapPlanarChunky { get; }

		/// <summary>
		/// Gets the property to work with image rotation for the current source.
		/// </summary>
		/// <value>
		/// The image rotation.
		/// </value>
		ICapWrapper<TWFix32> ICapRotation { get; }

		/// <summary>
		/// Gets the property to work with image shadow value for the current source.
		/// </summary>
		/// <value>
		/// The image shadow.
		/// </value>
		ICapWrapper<TWFix32> ICapShadow { get; }

		/// <summary>
		/// Gets the property to work with image barcode types for the current source.
		/// </summary>
		/// <value>
		/// The image barcode types.
		/// </value>
		IReadOnlyCapWrapper<BarcodeType> ICapSupportedBarcodeTypes { get; }

		/// <summary>
		/// Gets the property to get supported ext image info for the current source.
		/// </summary>
		/// <value>
		/// The supported ext image info.
		/// </value>
		IReadOnlyCapWrapper<ExtendedImageInfo> ICapSupportedExtImageInfo { get; }

		/// <summary>
		/// Gets the property to work with image patch code types for the current source.
		/// </summary>
		/// <value>
		/// The image patch code types.
		/// </value>
		IReadOnlyCapWrapper<PatchCode> ICapSupportedPatchCodeTypes { get; }

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.SupportedSize" /> for the current source.
		/// </summary>
		/// <value>
		/// The image supported size.
		/// </value>
		ICapWrapper<SupportedSize> ICapSupportedSizes { get; }

		/// <summary>
		/// Gets the property to work with image threshold for the current source.
		/// </summary>
		/// <value>
		/// The image threshold.
		/// </value>
		ICapWrapper<TWFix32> ICapThreshold { get; }

		/// <summary>
		/// Gets the property to work with image tiles flag for the current source.
		/// </summary>
		/// <value>
		/// The image tiles flag.
		/// </value>
		ICapWrapper<BoolType> ICapTiles { get; }

		/// <summary>
		/// Gets the property to work with image CCITT time fill for the current source.
		/// </summary>
		/// <value>
		/// The image CCITT time fill.
		/// </value>
		ICapWrapper<int> ICapTimeFill { get; }

		/// <summary>
		/// Gets the property to work with image undefined size flag for the current source.
		/// </summary>
		/// <value>
		/// The image undefined size flag.
		/// </value>
		ICapWrapper<BoolType> ICapUndefinedImageSize { get; }

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.Unit" /> for the current source.
		/// </summary>
		/// <value>
		/// The image unit of measure.
		/// </value>
		ICapWrapper<Unit> ICapUnits { get; }

		/// <summary>
		/// Gets the property to work with image <see cref="T:NTwain.Data.XferMech" /> for the current source.
		/// </summary>
		/// <value>
		/// The image xfer mech.
		/// </value>
		ICapWrapper<XferMech> ICapXferMech { get; }

		/// <summary>
		/// Gets the property to work with image's native x-axis resolution for the current source.
		/// </summary>
		/// <value>
		/// The image's native x-axis resolution.
		/// </value>
		IReadOnlyCapWrapper<TWFix32> ICapXNativeResolution { get; }

		/// <summary>
		/// Gets the property to work with image x-axis resolution for the current source.
		/// </summary>
		/// <value>
		/// The image x-axis resolution.
		/// </value>
		ICapWrapper<TWFix32> ICapXResolution { get; }

		/// <summary>
		/// Gets the property to work with image x-axis scaling for the current source.
		/// </summary>
		/// <value>
		/// The image x-axis scaling.
		/// </value>
		ICapWrapper<TWFix32> ICapXScaling { get; }

		/// <summary>
		/// Gets the property to work with image's native y-axis resolution for the current source.
		/// </summary>
		/// <value>
		/// The image's native y-axis resolution.
		/// </value>
		IReadOnlyCapWrapper<TWFix32> ICapYNativeResolution { get; }

		/// <summary>
		/// Gets the property to work with image y-axis resolution for the current source.
		/// </summary>
		/// <value>
		/// The image y-axis resolution.
		/// </value>
		ICapWrapper<TWFix32> ICapYResolution { get; }

		/// <summary>
		/// Gets the property to work with image y-axis scaling for the current source.
		/// </summary>
		/// <value>
		/// The image y-axis scaling.
		/// </value>
		ICapWrapper<TWFix32> ICapYScaling { get; }

		/// <summary>
		/// Gets the property to work with image zoom factor for the current source.
		/// </summary>
		/// <value>
		/// The image zoom factor.
		/// </value>
		ICapWrapper<int> ICapZoomFactor { get; }

		/// <summary>
		/// Gets the current value for a capability.
		/// </summary>
		/// <param name="capabilityId">The capability id.</param>
		/// <returns></returns>
		object GetCurrent(CapabilityId capabilityId);

		/// <summary>
		/// Gets the default value for a capability.
		/// </summary>
		/// <param name="capabilityId">The capability id.</param>
		/// <returns></returns>
		object GetDefault(CapabilityId capabilityId);

		/// <summary>
		/// A general method that tries to get capability values from current <see cref="T:NTwain.DataSource" />.
		/// </summary>
		/// <param name="capabilityId">The capability id.</param>
		/// <returns></returns>
		IEnumerable<object> GetValues(CapabilityId capabilityId);

		/// <summary>
		/// Gets all the possible values of this capability without expanding.
		/// This may be required to work with large range values that cannot be safely enumerated
		/// with <see cref="M:NTwain.ICapabilities.GetValues(NTwain.Data.CapabilityId)" />.
		/// </summary>
		/// <param name="capabilityId">The capability id.</param>
		/// <returns></returns>
		CapabilityReader GetValuesRaw(CapabilityId capabilityId);

		/// <summary>
		/// Gets the actual supported operations for a capability. This is not supported by all sources.
		/// </summary>
		/// <param name="capabilityId">The capability id.</param>
		/// <returns></returns>
		QuerySupports? QuerySupport(CapabilityId capabilityId);

		/// <summary>
		/// Resets the current value to power-on default.
		/// </summary>
		/// <param name="capabilityId">The capability id.</param>
		/// <returns></returns>
		ReturnCode Reset(CapabilityId capabilityId);

		/// <summary>
		/// Resets all values and constraint to power-on defaults.
		/// </summary>
		/// <returns></returns>
		ReturnCode ResetAll();
	}
	/// <summary>
	/// Interface for reading a TWAIN capability associated with a <see cref="T:NTwain.DataSource" />.
	/// </summary>
	/// <typeparam name="TValue">The TWAIN type of the value.</typeparam>    
	public interface IReadOnlyCapWrapper<TValue>
	{
		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.IReadOnlyCapWrapper`1.GetValues" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can get values; otherwise, <c>false</c>.
		/// </value>
		bool CanGet { get; }

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.IReadOnlyCapWrapper`1.GetDefault" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can get default value; otherwise, <c>false</c>.
		/// </value>
		bool CanGetCurrent { get; }

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.IReadOnlyCapWrapper`1.GetCurrent" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can get current value; otherwise, <c>false</c>.
		/// </value>
		bool CanGetDefault { get; }

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.IReadOnlyCapWrapper`1.GetHelp" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can get help; otherwise, <c>false</c>.
		/// </value>
		bool CanGetHelp { get; }

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.IReadOnlyCapWrapper`1.GetLabel" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can get label; otherwise, <c>false</c>.
		/// </value>
		bool CanGetLabel { get; }

		/// <summary>
		/// Gets a value indicating whether <see cref="M:NTwain.IReadOnlyCapWrapper`1.GetLabelEnum" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can get label enum; otherwise, <c>false</c>.
		/// </value>
		bool CanGetLabelEnum { get; }

		/// <summary>
		/// Gets a value indicating whether <see cref="!:Reset" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can reset; otherwise, <c>false</c>.
		/// </value>
		bool CanReset { get; }

		/// <summary>
		/// Gets a value indicating whether <see cref="!:Set" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can set; otherwise, <c>false</c>.
		/// </value>
		bool CanSet { get; }

		/// <summary>
		/// Gets a value indicating whether <see cref="!:SetConstraint" /> is supported.
		/// </summary>
		/// <value>
		///   <c>true</c> if this capability can set constraint; otherwise, <c>false</c>.
		/// </value>
		bool CanSetConstraint { get; }

		/// <summary>
		/// Gets the capability.
		/// </summary>
		/// <value>
		/// The capability.
		/// </value>
		CapabilityId Capability { get; }

		/// <summary>
		/// Gets a value indicating whether this capability is at all supported.
		/// </summary>
		/// <value>
		/// <c>true</c> if this capability is supported; otherwise, <c>false</c>.
		/// </value>
		bool IsSupported { get; }

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		bool IsReadOnly { get; }

		/// <summary>
		/// Gets all the possible values of this capability without expanding.
		/// This may be required to work with large range values that cannot be safely enumerated
		/// with <see cref="M:NTwain.IReadOnlyCapWrapper`1.GetValues" />.
		/// </summary>
		/// <returns></returns>
		CapabilityReader GetValuesRaw();

		/// <summary>
		/// Converts the object values into typed values using the conversion routine
		/// for this capability.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		IEnumerable<TValue> ConvertValues(IEnumerable<object> values);

		/// <summary>
		/// Gets all the possible values of this capability.
		/// </summary>
		/// <returns></returns>
		IEnumerable<TValue> GetValues();

		/// <summary>
		/// Gets the current value of this capability.
		/// </summary>
		/// <returns></returns>
		TValue GetCurrent();

		/// <summary>
		/// Gets the default value of this capability.
		/// </summary>
		/// <returns></returns>
		TValue GetDefault();

		/// <summary>
		/// [Experimental] Gets the help value of this capability.
		/// </summary>
		/// <returns></returns>
		string GetHelp();

		/// <summary>
		/// [Experimental] Gets the label value of this capability.
		/// </summary>
		/// <returns></returns>
		string GetLabel();

		/// <summary>
		/// [Experimental] Gets the display names for possible values of this capability.
		/// </summary>
		/// <returns></returns>
		IEnumerable<string> GetLabelEnum();
	}
	/// <summary>
	/// Interface for reading/writing a TWAIN capability associated with a <see cref="T:NTwain.DataSource" />.
	/// </summary>
	/// <typeparam name="TValue">The TWAIN type of the value.</typeparam>
	public interface ICapWrapper<TValue> : IReadOnlyCapWrapper<TValue>
	{
		/// <summary>
		/// Resets the current value to power-on default.
		/// </summary>
		/// <returns></returns>
		ReturnCode Reset();

		/// <summary>
		/// Simplified version that sets the current value of this capability.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		ReturnCode SetValue(TValue value);

		/// <summary>
		/// A version of Set that uses an array.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		ReturnCode SetValue(TWArray value);

		/// <summary>
		/// A version of Set that uses an enumeration.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		ReturnCode SetValue(TWEnumeration value);

		/// <summary>
		/// Sets the constraint value of this capability.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		ReturnCode SetConstraint(TWOneValue value);

		/// <summary>
		/// Sets the constraint value of this capability.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		ReturnCode SetConstraint(TWEnumeration value);

		/// <summary>
		/// Sets the constraint value of this capability.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		ReturnCode SetConstraint(TWRange value);
	}
	/// <summary>
	/// Represents a TWAIN data source.
	/// </summary>
	public interface IDataSource : ITripletControl
	{
		/// <summary>
		/// Gets the id of the source.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		int Id { get; }

		/// <summary>
		/// Gets the source's product name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		string Name { get; }

		/// <summary>
		/// Gets the supported data group.
		/// </summary>
		/// <value>
		/// The data group.
		/// </value>
		DataGroups DataGroup { get; }

		/// <summary>
		/// Gets the source's manufacturer name.
		/// </summary>
		/// <value>
		/// The manufacturer.
		/// </value>
		string Manufacturer { get; }

		/// <summary>
		/// Gets the source's product family.
		/// </summary>
		/// <value>
		/// The product family.
		/// </value>
		string ProductFamily { get; }

		/// <summary>
		/// Gets the supported TWAIN protocol version.
		/// </summary>
		/// <value>
		/// The protocol version.
		/// </value>
		Version ProtocolVersion { get; }

		/// <summary>
		/// Gets the source's version information.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
		TWVersion Version { get; }

		/// <summary>
		/// Gets a value indicating whether this data source has been opened.
		/// </summary>
		/// <value>
		///   <c>true</c> if this data source is open; otherwise, <c>false</c>.
		/// </value>
		bool IsOpen { get; }

		/// <summary>
		/// Gets the capabilities for this data source.
		/// </summary>
		/// <value>
		/// The capabilities.
		/// </value>
		ICapabilities Capabilities { get; }

		/// <summary>
		/// Gets or sets the current settings (CustomDSData) of this source if supported.
		/// </summary>
		/// <value>
		/// The source settings.
		/// </value>
		byte[] Settings { get; set; }

		/// <summary>
		/// Opens the source for capability negotiation.
		/// </summary>
		/// <returns></returns>
		ReturnCode Open();

		/// <summary>
		/// Closes the source.
		/// </summary>
		/// <returns></returns>
		ReturnCode Close();

		/// <summary>
		/// Enables the source to start transferring.
		/// </summary>
		/// <param name="mode">The mode.</param>
		/// <param name="modal">if set to <c>true</c> any driver UI will display as modal.</param>
		/// <param name="windowHandle">The window handle if modal.</param>
		/// <returns></returns>
		ReturnCode Enable(SourceEnableMode mode, bool modal, IntPtr windowHandle);

		/// <summary>
		/// Gets the source status. Only call this at state 4 or higher.
		/// </summary>
		/// <returns></returns>
		TWStatus GetStatus();

		/// <summary>
		/// Gets the source status string. Only call this at state 4 or higher.
		/// </summary>
		/// <param name="status">Status from previous calls.</param>
		/// <returns></returns>
		TWStatusUtf8 GetStatusUtf8(TWStatus status);
	}
	/// <summary>
	/// Simple log interface used by NTwain.
	/// </summary>
	public interface ILog
	{
		/// <summary>
		/// Gets or sets a value indicating whether info messages will be logged.
		/// </summary>
		/// <value>
		/// <c>true</c> to enable info logging.
		/// </value>
		bool IsInfoEnabled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether debug messages will be logged.
		/// </summary>
		/// <value>
		/// <c>true</c> to enable debug logging.
		/// </value>
		bool IsDebugEnabled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether error messages will be logged.
		/// </summary>
		/// <value>
		/// <c>true</c> to enable error logging.
		/// </value>
		bool IsErrorEnabled { get; set; }

		/// <summary>
		/// Logs info type message.
		/// </summary>
		/// <param name="message">The message.</param>
		void Info(string message);

		/// <summary>
		/// Logs info type message.
		/// </summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		void Info(string messageFormat, params object[] args);

		/// <summary>
		/// Logs debug type message.
		/// </summary>
		/// <param name="message">The message.</param>
		void Debug(string message);

		/// <summary>
		/// Logs debug type message.
		/// </summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		void Debug(string messageFormat, params object[] args);

		/// <summary>
		/// Logs error type message.
		/// </summary>
		/// <param name="message">The message.</param>
		void Error(string message);

		/// <summary>
		/// Logs error type message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="exception">The exception.</param>
		void Error(string message, Exception exception);

		/// <summary>
		/// Logs error type message.
		/// </summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="exception">The exception.</param>
		/// <param name="args">The arguments.</param>
		void Error(string messageFormat, Exception exception, params object[] args);
	}
	/// <summary>
	/// Interface that provides the correct methods for managing memory on data exchanged with TWAIN sources.
	/// </summary>
	public interface IMemoryManager
	{
		/// <summary>
		/// Function to allocate memory. Calls to this must be coupled with <see cref="M:NTwain.IMemoryManager.Free(System.IntPtr)" /> later.
		/// </summary>
		/// <param name="size">The size in bytes.</param>
		/// <returns>Handle to the allocated memory.</returns>
		IntPtr Allocate(uint size);

		/// <summary>
		/// Function to free memory. 
		/// </summary>
		/// <param name="handle">The handle from <see cref="M:NTwain.IMemoryManager.Allocate(System.UInt32)" />.</param>
		void Free(IntPtr handle);

		/// <summary>
		/// Function to lock some memory. Calls to this must be coupled with <see cref="M:NTwain.IMemoryManager.Unlock(System.IntPtr)" /> later.
		/// </summary>
		/// <param name="handle">The handle to allocated memory.</param>
		/// <returns>Handle to the lock.</returns>
		IntPtr Lock(IntPtr handle);

		/// <summary>
		/// Function to unlock a previously locked memory region.
		/// </summary>
		/// <param name="handle">The same handle passed <see cref="M:NTwain.IMemoryManager.Lock(System.IntPtr)" />.</param>
		void Unlock(IntPtr handle);
	}
	/// <summary>
	/// Contains various platform requirements and conditions for TWAIN.
	/// </summary>
	public interface IPlatformInfo
	{
		/// <summary>
		/// Gets a value indicating whether the applicable TWAIN DSM library exists in the operating system.
		/// </summary>
		/// <value>
		///   <c>true</c> if the TWAIN DSM; otherwise, <c>false</c>.
		/// </value>
		bool DsmExists { get; }

		/// <summary>
		/// Gets the expected TWAIN DSM dll path.
		/// </summary>
		/// <value>
		/// The expected DSM path.
		/// </value>
		string ExpectedDsmPath { get; }

		/// <summary>
		/// Gets or sets the twaindsm.dll path.
		/// </summary>
		/// <value>
		/// The DSM path.
		/// </value>
		string NewDsmPath { get; set; }

		/// <summary>
		/// Gets a value indicating whether the application is running in 64-bit.
		/// </summary>
		/// <value>
		/// <c>true</c> if the application is 64-bit; otherwise, <c>false</c>.
		/// </value>
		bool IsApp64Bit { get; }

		/// <summary>
		/// Gets a value indicating whether this library is supported on current OS.
		/// Check the other platform properties to determine the reason if this is false.
		/// </summary>
		/// <value>
		/// <c>true</c> if this library is supported; otherwise, <c>false</c>.
		/// </value>
		bool IsSupported { get; }

		/// <summary>
		/// Gets a value indicating whether the lib is expecting to use new DSM.
		/// </summary>
		/// <value>
		///   <c>true</c> if using the new DSM; otherwise, <c>false</c>.
		/// </value>
		bool UseNewWinDSM { get; }

		/// <summary>
		/// Gets or sets a value indicating whether to prefer using the new DSM on Windows over old twain_32 dsm if applicable.
		/// </summary>
		/// <value>
		///   <c>true</c> to prefer new DSM; otherwise, <c>false</c>.
		/// </value>
		bool PreferNewDSM { get; set; }

		/// <summary>
		/// Gets a value indicating whether the current runtime is mono.
		/// </summary>
		/// <value>
		/// <c>true</c> if the current runtime is on mono; otherwise, <c>false</c>.
		/// </value>
		bool IsOnMono { get; }

		/// <summary>
		/// Gets a value indicating whether the current OS is windows.
		/// </summary>
		/// <value>
		/// <c>true</c> if the current OS is windows; otherwise, <c>false</c>.
		/// </value>
		bool IsWindows { get; }

		/// <summary>
		/// Gets a value indicating whether the current OS is linux.
		/// </summary>
		/// <value>
		///   <c>true</c> if the current OS is linux; otherwise, <c>false</c>.
		/// </value>
		bool IsLinux { get; }

		/// <summary>
		/// Gets the <see cref="T:NTwain.IMemoryManager" /> for communicating with data sources.
		/// This should only be used when a <see cref="T:NTwain.TwainSession" /> is open.
		/// </summary>
		/// <value>
		/// The memory manager.
		/// </value>
		IMemoryManager MemoryManager { get; }

		/// <summary>
		/// Gets or sets the log used by NTwain.
		/// </summary>
		/// <value>
		/// The log.
		/// </value>
		ILog Log { get; set; }
	}
	/// <summary>
	/// Interface for providing TWAIN triplet access.
	/// </summary>
	public interface ITripletControl
	{
		/// <summary>
		/// Gets the triplet operations defined for control data group.
		/// </summary>
		DGControl DGControl { get; }

		/// <summary>
		/// Gets the triplet operations defined for image data group.
		/// </summary>
		DGImage DGImage { get; }

		/// <summary>
		/// Gets the direct triplet operation entry for custom values.
		/// </summary>
		DGCustom DGCustom { get; }
	}
	/// <summary>
	/// General interface for a TWAIN session.
	/// </summary>
	public interface ITwainSession : IEnumerable<DataSource>, IEnumerable, INotifyPropertyChanged
	{
		/// <summary>
		/// [Experimental] Gets or sets the optional synchronization context when not specifying a <see cref="T:NTwain.MessageLoopHook" /> on <see cref="M:NTwain.ITwainSession.Open" />.
		/// This allows events to be raised on the thread associated with the context. This is experimental is not recommended for use.
		/// </summary>
		/// <value>
		/// The synchronization context.
		/// </value>
		SynchronizationContext SynchronizationContext { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether calls to triplets will verify the current twain session state.
		/// </summary>
		/// <value>
		///   <c>true</c> if state value is enforced; otherwise, <c>false</c>.
		/// </value>
		bool EnforceState { get; set; }

		/// <summary>
		/// Gets the currently open source.
		/// </summary>
		/// <value>
		/// The current source.
		/// </value>
		DataSource CurrentSource { get; }

		/// <summary>
		/// Gets or sets the default source for this application.
		/// While this can be get as long as the session is open,
		/// it can only be set at State 3.
		/// </summary>
		/// <value>
		/// The default source.
		/// </value>
		DataSource DefaultSource { get; set; }

		/// <summary>
		/// Gets the current state number as defined by the TWAIN spec.
		/// </summary>
		/// <value>The state.</value>
		int State { get; }

		/// <summary>
		/// Gets the named state value as defined by the TWAIN spec.
		/// </summary>
		/// <value>
		/// The state.
		/// </value>
		State StateEx { get; }

		/// <summary>
		/// Quick flag to check if the DSM has been opened.
		/// </summary>
		bool IsDsmOpen { get; }

		/// <summary>
		/// Quick flag to check if a source has been opened.
		/// </summary>
		bool IsSourceOpen { get; }

		/// <summary>
		/// Quick flag to check if a source has been enabled.
		/// </summary>
		bool IsSourceEnabled { get; }

		/// <summary>
		/// Quick flag to check if a source is in the transferring state.
		/// </summary>
		bool IsTransferring { get; }

		/// <summary>
		/// Whether to stop the transfer process when transfer error is encountered.
		/// May be required on some sources.
		/// </summary>
		/// <value>
		/// <c>true</c> to stop on transfer error; otherwise, <c>false</c>.
		/// </value>
		bool StopOnTransferError { get; set; }

		/// <summary>
		/// Gets the reason a source was disabled (dropped from state 5) if it's due to user action.
		/// Mostly only <see cref="F:NTwain.Data.Message.CloseDSOK" /> or <see cref="F:NTwain.Data.Message.CloseDSReq" />.
		/// </summary>
		/// <value>
		/// The dialog result.
		/// </value>
		NTwain.Data.Message DisableReason { get; }

		/// <summary>
		/// Occurs when <see cref="P:NTwain.ITwainSession.State" /> has changed.
		/// </summary>
		event EventHandler StateChanged;

		/// <summary>
		/// Occurs when <see cref="P:NTwain.ITwainSession.CurrentSource" /> has changed.
		/// </summary>
		event EventHandler SourceChanged;

		/// <summary>
		/// Occurs when source has been disabled (back to state 4).
		/// </summary>
		event EventHandler SourceDisabled;

		/// <summary>
		/// Occurs when the source has generated an event.
		/// </summary>
		event EventHandler<DeviceEventArgs> DeviceEvent;

		/// <summary>
		/// Occurs when a data transfer is ready.
		/// </summary>
		event EventHandler<TransferReadyEventArgs> TransferReady;

		/// <summary>
		/// Occurs when data has been transferred.
		/// </summary>
		event EventHandler<DataTransferredEventArgs> DataTransferred;

		/// <summary>
		/// Occurs when an error has been encountered during transfer.
		/// </summary>
		event EventHandler<TransferErrorEventArgs> TransferError;

		/// <summary>
		/// Try to show the built-in source selector dialog and return the selected source.
		/// This is not recommended and is only included for completeness.
		/// </summary>
		/// <returns></returns>
		DataSource ShowSourceSelector();

		/// <summary>
		/// Opens the data source manager. This must be the first method used
		/// before using other TWAIN functions. Calls to this must be followed by 
		/// <see cref="M:NTwain.ITwainSession.Close" /> when done with a TWAIN session.
		/// </summary>
		/// <returns></returns>
		ReturnCode Open();

		/// <summary>
		/// Opens the data source manager. This must be the first method used
		/// before using other TWAIN functions. Calls to this must be followed by 
		/// <see cref="M:NTwain.ITwainSession.Close" /> when done with a TWAIN session.
		/// </summary>
		/// <param name="messageLoopHook">The message loop hook.</param>
		/// <returns></returns>
		ReturnCode Open(MessageLoopHook messageLoopHook);

		/// <summary>
		/// Closes the data source manager.
		/// </summary>
		/// <returns></returns>
		ReturnCode Close();

		/// <summary>
		/// Forces the stepping down of an opened source when things gets out of control.
		/// Used when session state and source state become out of sync.
		/// </summary>
		/// <param name="targetState">State of the target.</param>
		void ForceStepDown(int targetState);

		/// <summary>
		/// Gets list of sources available in the system.
		/// </summary>
		/// <returns></returns>
		IEnumerable<DataSource> GetSources();

		/// <summary>
		/// Quick shortcut to open a source.
		/// </summary>
		/// <param name="sourceName">Name of the source.</param>
		/// <returns></returns>
		ReturnCode OpenSource(string sourceName);

		/// <summary>
		/// Quick shortcut to open a source.
		/// </summary>
		/// <param name="sourceId">Id of the source.</param>
		/// <returns></returns>
		ReturnCode OpenSource(int sourceId);

		/// <summary>
		/// Gets the manager status. Only call this at state 2 or higher.
		/// </summary>
		/// <returns></returns>
		TWStatus GetStatus();

		/// <summary>
		/// Gets the manager status string. Only call this at state 3 or higher.
		/// </summary>
		/// <param name="status">Status from previous calls.</param>
		/// <returns></returns>
		TWStatusUtf8 GetStatusUtf8(TWStatus status);
	}
	/// <summary>
	/// An abstract class for TWAIN to hook into windows message loops.
	/// </summary>
	public abstract class MessageLoopHook
	{
		/// <summary>
		/// Interface for something that can check whether messages from WndProc is a TWAIN message and already handled.
		/// </summary>
		protected internal interface IWinMessageFilter
		{
			/// <summary>
			/// Checks and handle the message if it's a TWAIN message.
			/// </summary>
			/// <param name="hwnd">The window handle.</param>
			/// <param name="msg">The message.</param>
			/// <param name="wParam">The w parameter.</param>
			/// <param name="lParam">The l parameter.</param>
			/// <returns>true if handled internally.</returns>
			bool IsTwainMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
		}

		protected internal IntPtr Handle { get; set; }

		protected internal SynchronizationContext SyncContext { get; set; }

		internal virtual string InvalidMessage => string.Empty;

		protected internal abstract void Start(IWinMessageFilter filter);

		protected internal abstract void Stop();

		/// <summary>
		/// Asynchronously invokes the specified action on the message loop thread.
		/// </summary>
		/// <param name="action">The action.</param>
		public virtual void BeginInvoke(Action action)
		{
			if (SyncContext == null)
			{
				action();
				return;
			}
			SyncContext.Post((object o) =>
			{
				action();
			}, null);
		}

		/// <summary>
		/// Synchronously invokes the specified action on the message loop thread.
		/// </summary>
		/// <param name="action">The action.</param>
		public virtual void Invoke(Action action)
		{
			if (SyncContext == null)
			{
				action();
				return;
			}
			Exception error = null;
			SyncContext.Send((object o) =>
			{
				try
				{
					action();
				}
				catch (Exception ex)
				{
					error = ex;
				}
			}, null);
			if (error != null)
			{
				Rethrow(error);
			}
		}

		/// <summary>
		/// Rethrows the specified excetion while keeping stack trace.
		/// </summary>
		/// <param name="ex">The ex.</param>
		protected static void Rethrow(Exception ex)
		{
			typeof(Exception).GetMethod("PrepForRemoting", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(ex, new object[0]);
			throw ex;
		}

		internal void ThrowInvalidOp()
		{
			throw new InvalidOperationException(InvalidMessage);
		}
	}
	/// <summary>
	/// A <see cref="T:NTwain.MessageLoopHook" /> for use in winform applications.
	/// </summary>
	public sealed class WindowsFormsMessageLoopHook : MessageLoopHook, IMessageFilter
	{
		private IWinMessageFilter _filter;

		internal override string InvalidMessage => "This can only be created on the Windows Forms UI thread.";

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.WindowsFormsMessageLoopHook" /> class.
		/// </summary>
		/// <param name="windowHandle">The handle to the app window.</param>
		/// <exception cref="T:System.ArgumentException">A valid window handle is required.</exception>
		public WindowsFormsMessageLoopHook(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero)
			{
				throw new ArgumentException("A valid window handle is required.");
			}
			if (!System.Windows.Forms.Application.MessageLoop)
			{
				ThrowInvalidOp();
			}
			SynchronizationContext current = SynchronizationContext.Current;
			if (current == null)
			{
				ThrowInvalidOp();
			}
			Handle = windowHandle;
			SyncContext = current;
		}

		protected internal override void Start(IWinMessageFilter filter)
		{
			_filter = filter;
			System.Windows.Forms.Application.AddMessageFilter(this);
		}

		protected internal override void Stop()
		{
			System.Windows.Forms.Application.RemoveMessageFilter(this);
			_filter = null;
		}

		bool IMessageFilter.PreFilterMessage(ref System.Windows.Forms.Message m)
		{
			if (_filter != null)
			{
				return _filter.IsTwainMessage(m.HWnd, m.Msg, m.WParam, m.LParam);
			}
			return false;
		}
	}
	/// <summary>
	/// A <see cref="T:NTwain.MessageLoopHook" /> for use in WPF applications.
	/// </summary>
	public sealed class WpfMessageLoopHook : MessageLoopHook
	{
		private HwndSource _hooker;

		private IWinMessageFilter _filter;

		internal override string InvalidMessage => "This can only be created on the WPF UI thread.";

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.WpfMessageLoopHook" /> class.
		/// </summary>
		/// <param name="windowHandle">The handle to the app window.</param>
		/// <exception cref="T:System.ArgumentException">A valid window handle is required.</exception>
		public WpfMessageLoopHook(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero)
			{
				throw new ArgumentException("A valid window handle is required.");
			}
			if (System.Windows.Application.Current == null || !System.Windows.Application.Current.Dispatcher.CheckAccess())
			{
				ThrowInvalidOp();
			}
			SynchronizationContext current = SynchronizationContext.Current;
			if (current == null)
			{
				ThrowInvalidOp();
			}
			Handle = windowHandle;
			SyncContext = current;
		}

		protected internal override void Start(IWinMessageFilter filter)
		{
			_filter = filter;
			_hooker = HwndSource.FromHwnd(Handle);
			_hooker.AddHook(FilterMessage);
		}

		private IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (_filter != null)
			{
				handled = _filter.IsTwainMessage(hwnd, msg, wParam, lParam);
			}
			return IntPtr.Zero;
		}

		protected internal override void Stop()
		{
			if (_hooker != null)
			{
				_hooker.RemoveHook(FilterMessage);
				_hooker = null;
			}
		}
	}
	/// <summary>
	/// Contains various platform requirements and conditions for TWAIN.
	/// </summary>
	public class PlatformInfo : IPlatformInfo
	{
		private static readonly PlatformInfo __global = new PlatformInfo();

		private string oldDsmPath;

		private string newDsmPath;

		private bool _preferNewDSM;

		private readonly IMemoryManager _defaultMemManager;

		private IMemoryManager _specifiedMemManager;

		private readonly ILog _defaultLog;

		private ILog _log;

		internal static PlatformInfo InternalCurrent => __global;

		/// <summary>
		/// Gets the current platform info related to TWAIN.
		/// </summary>
		/// <value>
		/// The current info.
		/// </value>
		public static IPlatformInfo Current => __global;

		/// <summary>
		/// Gets a value indicating whether to prefer using the new DSM on Windows over old twain_32 dsm if applicable.
		/// </summary>
		/// <value>
		///   <c>true</c> to prefer new DSM; otherwise, <c>false</c>.
		/// </value>
		public bool PreferNewDSM
		{
			get
			{
				return _preferNewDSM;
			}
			set
			{
				if (IsWindows)
				{
					_preferNewDSM = value;
					if (IsApp64Bit)
					{
						ExpectedDsmPath = newDsmPath;
						bool isSupported = (DsmExists = File.Exists(ExpectedDsmPath));
						IsSupported = isSupported;
						UseNewWinDSM = true;
						Log.Debug("Using new dsm.");
					}
					else if (_preferNewDSM && File.Exists(newDsmPath))
					{
						ExpectedDsmPath = newDsmPath;
						bool flag2 = (DsmExists = true);
						bool isSupported = (IsSupported = flag2);
						UseNewWinDSM = isSupported;
						Log.Debug("Using new dsm.");
					}
					else
					{
						ExpectedDsmPath = oldDsmPath;
						bool isSupported = (DsmExists = File.Exists(ExpectedDsmPath));
						IsSupported = isSupported;
						UseNewWinDSM = false;
						Log.Debug("Using old dsm in windows. not " + newDsmPath);
					}
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the lib is expecting to use new DSM.
		/// </summary>
		/// <value>
		///   <c>true</c> if using the new DSM; otherwise, <c>false</c>.
		/// </value>
		public bool UseNewWinDSM { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the current runtime is mono.
		/// </summary>
		/// <value>
		/// <c>true</c> if the current runtime is on mono; otherwise, <c>false</c>.
		/// </value>
		public bool IsOnMono { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the current OS is windows.
		/// </summary>
		/// <value>
		/// <c>true</c> if the current OS is windows; otherwise, <c>false</c>.
		/// </value>
		public bool IsWindows { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the current OS is linux.
		/// </summary>
		/// <value>
		///   <c>true</c> if the current OS is linux; otherwise, <c>false</c>.
		/// </value>
		public bool IsLinux { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the application is running in 64-bit.
		/// </summary>
		/// <value>
		/// <c>true</c> if the application is 64-bit; otherwise, <c>false</c>.
		/// </value>
		public bool IsApp64Bit { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the applicable TWAIN DSM library exists in the operating system.
		/// </summary>
		/// <value>
		///   <c>true</c> if the TWAIN DSM; otherwise, <c>false</c>.
		/// </value>
		public bool DsmExists { get; private set; }

		/// <summary>
		/// Gets the expected TWAIN DSM dll path.
		/// </summary>
		/// <value>
		/// The expected DSM path.
		/// </value>
		public string ExpectedDsmPath { get; private set; }

		/// <summary>
		/// Gets or sets the user-provided TWAIN DSM dll path.
		/// </summary>
		/// <value>
		/// The DSM path.
		/// </value>
		public string NewDsmPath
		{
			get
			{
				return newDsmPath;
			}
			set
			{
				newDsmPath = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this library is supported on current OS.
		/// Check the other platform properties to determine the reason if this is false.
		/// </summary>
		/// <value>
		/// <c>true</c> if this library is supported; otherwise, <c>false</c>.
		/// </value>
		public bool IsSupported { get; private set; }

		/// <summary>
		/// Gets the <see cref="T:NTwain.IMemoryManager" /> for communicating with data sources.
		/// This should only be used when a <see cref="T:NTwain.TwainSession" /> is open.
		/// </summary>
		/// <value>
		/// The memory manager.
		/// </value>
		public IMemoryManager MemoryManager
		{
			get
			{
				return _specifiedMemManager ?? _defaultMemManager;
			}
			internal set
			{
				_specifiedMemManager = value;
			}
		}

		/// <summary>
		/// Gets or sets the log used by NTwain.
		/// </summary>
		/// <value>
		/// The log.
		/// </value>
		public ILog Log
		{
			get
			{
				return _log ?? _defaultLog;
			}
			set
			{
				_log = value;
			}
		}

		private PlatformInfo()
		{
			IsApp64Bit = IntPtr.Size == 8;
			IsOnMono = Type.GetType("Mono.Runtime") != null;
			IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
			IsLinux = Environment.OSVersion.Platform == PlatformID.Unix;
			_defaultLog = new TraceLog();
			if (IsWindows)
			{
				_defaultMemManager = new WinMemoryManager();
				newDsmPath = GetFirstFilePathThatExists("twaindsm.dll", GetExeFolder(), AppDomain.CurrentDomain.BaseDirectory, Environment.SystemDirectory, GetWindowsFolder(), Environment.CurrentDirectory) ?? Path.Combine(Environment.SystemDirectory, "twaindsm.dll");
				oldDsmPath = Path.Combine(GetWindowsFolder(), "twain_32.dll");
				PreferNewDSM = true;
			}
			else if (IsLinux)
			{
				_defaultMemManager = new LinuxMemoryManager();
				ExpectedDsmPath = "/usr/local/lib/libtwaindsm.so";
				DsmExists = File.Exists(ExpectedDsmPath);
				IsSupported = DsmExists && IsOnMono;
			}
		}

		private string GetExeFolder()
		{
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			if (entryAssembly != null)
			{
				return Path.GetDirectoryName(entryAssembly.Location);
			}
			return null;
		}

		private string GetWindowsFolder()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.Windows);
		}

		private static string GetFirstFilePathThatExists(string filename, params string[] folders)
		{
			return (from fdr in folders
				where fdr != null
				select Path.Combine(fdr, filename)).FirstOrDefault((string path) => File.Exists(path));
		}
	}
	/// <summary>
	/// Contains the minimum TWAIN protocol version for various things.
	/// </summary>
	public static class ProtocolVersions
	{
		internal static readonly Version v10 = new Version(1, 0);

		internal static readonly Version v11 = new Version(1, 1);

		internal static readonly Version v15 = new Version(1, 5);

		internal static readonly Version v16 = new Version(1, 6);

		internal static readonly Version v17 = new Version(1, 7);

		internal static readonly Version v18 = new Version(1, 8);

		internal static readonly Version v19 = new Version(1, 9);

		internal static readonly Version v20 = new Version(2, 0);

		internal static readonly Version v21 = new Version(2, 1);

		internal static readonly Version v22 = new Version(2, 2);

		internal static readonly Version v23 = new Version(2, 3);

		private static readonly Dictionary<CapabilityId, Version> __capMinVersions = new Dictionary<CapabilityId, Version>
		{
			{
				CapabilityId.ACapXferMech,
				v18
			},
			{
				CapabilityId.CapAlarms,
				v18
			},
			{
				CapabilityId.CapAlarmVolume,
				v18
			},
			{
				CapabilityId.CapAuthor,
				v10
			},
			{
				CapabilityId.CapAutoFeed,
				v10
			},
			{
				CapabilityId.CapAutomaticCapture,
				v18
			},
			{
				CapabilityId.CapAutomaticSenseMedium,
				v21
			},
			{
				CapabilityId.CapAutoScan,
				v16
			},
			{
				CapabilityId.CapBatteryMinutes,
				v18
			},
			{
				CapabilityId.CapBatteryPercentage,
				v18
			},
			{
				CapabilityId.CapCameraEnabled,
				v20
			},
			{
				CapabilityId.CapCameraOrder,
				v20
			},
			{
				CapabilityId.CapCameraPreviewUI,
				v18
			},
			{
				CapabilityId.CapCameraSide,
				v19
			},
			{
				CapabilityId.CapCaption,
				v10
			},
			{
				CapabilityId.CapClearBuffers,
				v18
			},
			{
				CapabilityId.CapClearPage,
				v10
			},
			{
				CapabilityId.CapCustomDSData,
				v17
			},
			{
				CapabilityId.CapCustomInterfaceGuid,
				v21
			},
			{
				CapabilityId.CapDeviceEvent,
				v18
			},
			{
				CapabilityId.CapDeviceOnline,
				v16
			},
			{
				CapabilityId.CapDeviceTimeDate,
				v18
			},
			{
				CapabilityId.CapDoubleFeedDetection,
				v22
			},
			{
				CapabilityId.CapDoubleFeedDetectionLength,
				v22
			},
			{
				CapabilityId.CapDoubleFeedDetectionResponse,
				v22
			},
			{
				CapabilityId.CapDoubleFeedDetectionSensitivity,
				v22
			},
			{
				CapabilityId.CapDuplex,
				v17
			},
			{
				CapabilityId.CapDuplexEnabled,
				v17
			},
			{
				CapabilityId.CapEnableDSUIOnly,
				v17
			},
			{
				CapabilityId.CapEndorser,
				v17
			},
			{
				CapabilityId.CapExtendedCaps,
				v10
			},
			{
				CapabilityId.CapFeederAlignment,
				v18
			},
			{
				CapabilityId.CapFeederEnabled,
				v10
			},
			{
				CapabilityId.CapFeederLoaded,
				v10
			},
			{
				CapabilityId.CapFeederOrder,
				v18
			},
			{
				CapabilityId.CapFeederPocket,
				v20
			},
			{
				CapabilityId.CapFeederPrep,
				v20
			},
			{
				CapabilityId.CapFeedPage,
				v10
			},
			{
				CapabilityId.CapIndicators,
				v11
			},
			{
				CapabilityId.CapIndicatorsMode,
				v22
			},
			{
				CapabilityId.CapJobControl,
				v17
			},
			{
				CapabilityId.CapLanguage,
				v18
			},
			{
				CapabilityId.CapMaxBatchBuffers,
				v18
			},
			{
				CapabilityId.CapMicrEnabled,
				v20
			},
			{
				CapabilityId.CapPaperDetectable,
				v16
			},
			{
				CapabilityId.CapPaperHandling,
				v22
			},
			{
				CapabilityId.CapPowerSaveTime,
				v18
			},
			{
				CapabilityId.CapPowerSupply,
				v18
			},
			{
				CapabilityId.CapPrinter,
				v18
			},
			{
				CapabilityId.CapPrinterEnabled,
				v18
			},
			{
				CapabilityId.CapPrinterCharRotation,
				v23
			},
			{
				CapabilityId.CapPrinterFontStyle,
				v23
			},
			{
				CapabilityId.CapPrinterIndex,
				v18
			},
			{
				CapabilityId.CapPrinterIndexLeadChar,
				v23
			},
			{
				CapabilityId.CapPrinterIndexMaxValue,
				v23
			},
			{
				CapabilityId.CapPrinterIndexNumDigits,
				v23
			},
			{
				CapabilityId.CapPrinterIndexStep,
				v23
			},
			{
				CapabilityId.CapPrinterIndexTrigger,
				v23
			},
			{
				CapabilityId.CapPrinterMode,
				v18
			},
			{
				CapabilityId.CapPrinterString,
				v18
			},
			{
				CapabilityId.CapPrinterStringPreview,
				v23
			},
			{
				CapabilityId.CapPrinterSuffix,
				v18
			},
			{
				CapabilityId.CapPrinterVerticalOffset,
				v22
			},
			{
				CapabilityId.CapReacquireAllowed,
				v18
			},
			{
				CapabilityId.CapRewindPage,
				v10
			},
			{
				CapabilityId.CapSegmented,
				v19
			},
			{
				CapabilityId.CapSerialNumber,
				v18
			},
			{
				CapabilityId.CapSupportedCaps,
				v10
			},
			{
				CapabilityId.CapSupportedCapsSegmentUnique,
				v22
			},
			{
				CapabilityId.CapSupportedDATs,
				v22
			},
			{
				CapabilityId.CapTimeBeforeFirstCapture,
				v18
			},
			{
				CapabilityId.CapTimeBetweenCaptures,
				v18
			},
			{
				CapabilityId.CapTimeDate,
				v10
			},
			{
				CapabilityId.CapThumbnailsEnabled,
				v17
			},
			{
				CapabilityId.CapUIControllable,
				v16
			},
			{
				CapabilityId.CapXferCount,
				v10
			},
			{
				CapabilityId.ICapAutoBright,
				v10
			},
			{
				CapabilityId.ICapAutoDiscardBlankPages,
				v20
			},
			{
				CapabilityId.ICapAutomaticBorderDetection,
				v18
			},
			{
				CapabilityId.ICapAutomaticColorEnabled,
				v21
			},
			{
				CapabilityId.ICapAutomaticColorNonColorPixelType,
				v18
			},
			{
				CapabilityId.ICapAutomaticCropUsesFrame,
				v21
			},
			{
				CapabilityId.ICapAutomaticDeskew,
				v18
			},
			{
				CapabilityId.ICapAutomaticLengthDetection,
				v21
			},
			{
				CapabilityId.ICapAutomaticRotate,
				v18
			},
			{
				CapabilityId.ICapAutoSize,
				v20
			},
			{
				CapabilityId.ICapBarcodeDetectionEnabled,
				v18
			},
			{
				CapabilityId.ICapBarcodeMaxRetries,
				v18
			},
			{
				CapabilityId.ICapBarcodeMaxSearchPriorities,
				v18
			},
			{
				CapabilityId.ICapBarcodeSearchMode,
				v18
			},
			{
				CapabilityId.ICapBarcodeSearchPriorities,
				v18
			},
			{
				CapabilityId.ICapBarcodeTimeout,
				v18
			},
			{
				CapabilityId.ICapBitDepth,
				v10
			},
			{
				CapabilityId.ICapBitDepthReduction,
				v15
			},
			{
				CapabilityId.ICapBitOrder,
				v10
			},
			{
				CapabilityId.ICapBitOrderCodes,
				v10
			},
			{
				CapabilityId.ICapBrightness,
				v10
			},
			{
				CapabilityId.ICapCCITTKFactor,
				v10
			},
			{
				CapabilityId.ICapColorManagementEnabled,
				v21
			},
			{
				CapabilityId.ICapCompression,
				v10
			},
			{
				CapabilityId.ICapContrast,
				v10
			},
			{
				CapabilityId.ICapCustHalftone,
				v10
			},
			{
				CapabilityId.ICapExposureTime,
				v10
			},
			{
				CapabilityId.ICapExtImageInfo,
				v17
			},
			{
				CapabilityId.ICapFeederType,
				v19
			},
			{
				CapabilityId.ICapFilmType,
				v22
			},
			{
				CapabilityId.ICapFilter,
				v10
			},
			{
				CapabilityId.ICapFlashUsed,
				v16
			},
			{
				CapabilityId.ICapFlashUsed2,
				v18
			},
			{
				CapabilityId.ICapFlipRotation,
				v18
			},
			{
				CapabilityId.ICapFrames,
				v10
			},
			{
				CapabilityId.ICapGamma,
				v10
			},
			{
				CapabilityId.ICapHalftones,
				v10
			},
			{
				CapabilityId.ICapHighlight,
				v10
			},
			{
				CapabilityId.ICapICCProfile,
				v19
			},
			{
				CapabilityId.ICapImageDataSet,
				v17
			},
			{
				CapabilityId.ICapImageFileFormat,
				v10
			},
			{
				CapabilityId.ICapImageFilter,
				v18
			},
			{
				CapabilityId.ICapImageMerge,
				v21
			},
			{
				CapabilityId.ICapImageMergeHeightThreshold,
				v21
			},
			{
				CapabilityId.ICapJpegPixelType,
				v10
			},
			{
				CapabilityId.ICapJpegQuality,
				v19
			},
			{
				CapabilityId.ICapJpegSubsampling,
				v22
			},
			{
				CapabilityId.ICapLampState,
				v10
			},
			{
				CapabilityId.ICapLightPath,
				v10
			},
			{
				CapabilityId.ICapLightSource,
				v10
			},
			{
				CapabilityId.ICapMaxFrames,
				v10
			},
			{
				CapabilityId.ICapMinimumHeight,
				v17
			},
			{
				CapabilityId.ICapMinimumWidth,
				v17
			},
			{
				CapabilityId.ICapMirror,
				v22
			},
			{
				CapabilityId.ICapNoiseFilter,
				v18
			},
			{
				CapabilityId.ICapOrientation,
				v10
			},
			{
				CapabilityId.ICapOverScan,
				v18
			},
			{
				CapabilityId.ICapPatchCodeDetectionEnabled,
				v18
			},
			{
				CapabilityId.ICapPatchCodeMaxRetries,
				v18
			},
			{
				CapabilityId.ICapPatchCodeMaxSearchPriorities,
				v18
			},
			{
				CapabilityId.ICapPatchCodeSearchMode,
				v18
			},
			{
				CapabilityId.ICapPatchCodeSearchPriorities,
				v18
			},
			{
				CapabilityId.ICapPatchCodeTimeout,
				v18
			},
			{
				CapabilityId.ICapPhysicalHeight,
				v10
			},
			{
				CapabilityId.ICapPhysicalWidth,
				v10
			},
			{
				CapabilityId.ICapPixelFlavor,
				v10
			},
			{
				CapabilityId.ICapPixelFlavorCodes,
				v10
			},
			{
				CapabilityId.ICapPixelType,
				v10
			},
			{
				CapabilityId.ICapPlanarChunky,
				v10
			},
			{
				CapabilityId.ICapRotation,
				v10
			},
			{
				CapabilityId.ICapShadow,
				v10
			},
			{
				CapabilityId.ICapSupportedBarcodeTypes,
				v18
			},
			{
				CapabilityId.ICapSupportedExtImageInfo,
				v21
			},
			{
				CapabilityId.ICapSupportedPatchCodeTypes,
				v18
			},
			{
				CapabilityId.ICapSupportedSizes,
				v10
			},
			{
				CapabilityId.ICapThreshold,
				v10
			},
			{
				CapabilityId.ICapTiles,
				v10
			},
			{
				CapabilityId.ICapTimeFill,
				v10
			},
			{
				CapabilityId.ICapUndefinedImageSize,
				v16
			},
			{
				CapabilityId.ICapUnits,
				v10
			},
			{
				CapabilityId.ICapXferMech,
				v10
			},
			{
				CapabilityId.ICapXNativeResolution,
				v10
			},
			{
				CapabilityId.ICapXResolution,
				v10
			},
			{
				CapabilityId.ICapXScaling,
				v10
			},
			{
				CapabilityId.ICapYNativeResolution,
				v10
			},
			{
				CapabilityId.ICapYResolution,
				v10
			},
			{
				CapabilityId.ICapYScaling,
				v10
			},
			{
				CapabilityId.ICapZoomFactor,
				v18
			}
		};

		/// <summary>
		/// Gets the minimum TWAIN protocol version for a capability.
		/// </summary>
		/// <param name="id">The capability type.</param>
		/// <returns></returns>
		public static Version GetMinimumVersion(CapabilityId id)
		{
			if (__capMinVersions.ContainsKey(id))
			{
				return __capMinVersions[id];
			}
			return v10;
		}
	}
	/// <summary>
	/// Indicates how the source should be enabled in a TWAIN session.
	/// </summary>
	public enum SourceEnableMode
	{
		/// <summary>
		/// Start acquiring without driver UI.
		/// </summary>
		NoUI,
		/// <summary>
		/// Start acquiring with driver UI.
		/// </summary>
		ShowUI,
		/// <summary>
		/// Show driver UI for settings change but no acquisition.
		/// </summary>
		ShowUIOnly
	}
	/// <summary>
	/// The logical state of a TWAIN session.
	/// </summary>
	public enum State
	{
		/// <summary>
		/// Just a default value.
		/// </summary>
		Invalid,
		/// <summary>
		/// The starting state, corresponds to state 1.
		/// </summary>
		DsmUnloaded,
		/// <summary>
		/// The DSM library has been loaded, corresponds to state 2.
		/// </summary>
		DsmLoaded,
		/// <summary>
		/// The DSM has been opened, corresponds to state 3.
		/// </summary>
		DsmOpened,
		/// <summary>
		/// A data source has been opened, corresponds to state 4.
		/// </summary>
		SourceOpened,
		/// <summary>
		/// A data source has been enabled, corresponds to state 5.
		/// </summary>
		SourceEnabled,
		/// <summary>
		/// Data is ready for transfer from the source, corresponds to state 6.
		/// </summary>
		TransferReady,
		/// <summary>
		/// Data is being transferred, corresponds to state 7.
		/// </summary>
		Transferring
	}
	/// <summary>
	/// Indicates a transfer cancellation, e.g. if the user pressed the "Cancel" button.
	/// </summary>
	public class TransferCanceledEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TransferCanceledEventArgs" /> class.
		/// </summary>
		public TransferCanceledEventArgs()
		{
		}
	}
	/// <summary>
	/// Contains TWAIN codes and source status when an error is encountered during transfer.
	/// </summary>
	public class TransferErrorEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the return code from the transfer.
		/// </summary>
		/// <value>
		/// The return code.
		/// </value>
		public ReturnCode ReturnCode { get; private set; }

		/// <summary>
		/// Gets the source status.
		/// </summary>
		/// <value>
		/// The source status.
		/// </value>
		public TWStatus SourceStatus { get; private set; }

		/// <summary>
		/// Gets the exception if the error is from some exception.
		/// </summary>
		/// <value>
		/// The exception.
		/// </value>
		public Exception Exception { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TransferErrorEventArgs" /> class.
		/// </summary>
		/// <param name="error">The error.</param>
		public TransferErrorEventArgs(Exception error)
		{
			Exception = error;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TransferErrorEventArgs" /> class.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="status">The status.</param>
		public TransferErrorEventArgs(ReturnCode code, TWStatus status)
		{
			ReturnCode = code;
			SourceStatus = status;
		}
	}
	/// <summary>
	/// Contains event data when a data transfer is ready to be processed.
	/// </summary>
	public class TransferReadyEventArgs : EventArgs
	{
		private TWImageInfo _imgInfo;

		private TWAudioInfo _audInfo;

		/// <summary>
		/// Gets the data source.
		/// </summary>
		/// <value>
		/// The data source.
		/// </value>
		public DataSource DataSource { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether the current transfer should be canceled
		/// and continue next transfer if there are more data.
		/// </summary>
		/// <value><c>true</c> to cancel current transfer; otherwise, <c>false</c>.</value>
		public bool CancelCurrent { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether all transfers should be canceled.
		/// </summary>
		/// <value><c>true</c> to cancel all transfers; otherwise, <c>false</c>.</value>
		public bool CancelAll { get; set; }

		/// <summary>
		/// Gets a value indicating whether current transfer signifies an end of job in TWAIN world.
		/// </summary>
		/// <value><c>true</c> if transfer is end of job; otherwise, <c>false</c>.</value>
		public bool EndOfJob => EndOfJobFlag != EndXferJob.None;

		/// <summary>
		/// Gets the end of job flag value for this transfer (if job control is enabled).
		/// </summary>
		/// <value>
		/// The end of job flag.
		/// </value>
		public EndXferJob EndOfJobFlag { get; private set; }

		/// <summary>
		/// Gets the known pending transfer count. This may not be appilicable 
		/// for certain scanning modes.
		/// </summary>
		/// <value>The pending count.</value>
		public int PendingTransferCount { get; private set; }

		/// <summary>
		/// Gets the tentative image information for the current transfer if applicable.
		/// This may differ from the final image depending on the transfer mode used (mostly when doing mem xfer).
		/// </summary>
		/// <value>
		/// The image info.
		/// </value>
		public TWImageInfo PendingImageInfo
		{
			get
			{
				if (_imgInfo == null && DataSource.DGImage.ImageInfo.Get(out _imgInfo) != ReturnCode.Success)
				{
					_imgInfo = null;
				}
				return _imgInfo;
			}
		}

		/// <summary>
		/// Gets the audio information for the current transfer if applicable.
		/// </summary>
		/// <value>
		/// The audio information.
		/// </value>
		public TWAudioInfo AudioInfo
		{
			get
			{
				if (_audInfo == null && DataSource.DGAudio.AudioInfo.Get(out _audInfo) != ReturnCode.Success)
				{
					_audInfo = null;
				}
				return _audInfo;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TransferReadyEventArgs" /> class.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="pendingCount">The pending count.</param>
		/// <param name="endOfJobFlag"></param>
		public TransferReadyEventArgs(DataSource source, int pendingCount, EndXferJob endOfJobFlag)
		{
			DataSource = source;
			PendingTransferCount = pendingCount;
			EndOfJobFlag = endOfJobFlag;
		}
	}
	/// <summary>
	/// Represents a general exception with TWAIN.
	/// </summary>
	[Serializable]
	public class TwainException : Exception
	{
		/// <summary>
		/// Gets the return code from the TWAIN operation if applicable.
		/// </summary>
		/// <value>
		/// The return code.
		/// </value>
		public ReturnCode ReturnCode { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TwainException" /> class.
		/// </summary>
		public TwainException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TwainException" /> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public TwainException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TwainException" /> class.
		/// </summary>
		/// <param name="returnCode">The return code.</param>
		/// <param name="message">The message.</param>
		public TwainException(ReturnCode returnCode, string message)
			: this(returnCode, message, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TwainException" /> class.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
		public TwainException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TwainException" /> class.
		/// </summary>
		/// <param name="returnCode">The return code.</param>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public TwainException(ReturnCode returnCode, string message, Exception innerException)
			: base(message, innerException)
		{
			ReturnCode = returnCode;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TwainException" /> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="info" /> parameter is null.
		/// </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">
		/// The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0).
		/// </exception>
		protected TwainException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info != null)
			{
				ReturnCode = (ReturnCode)info.GetUInt16("RC");
			}
		}

		/// <summary>
		/// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="info" /> parameter is a null reference (Nothing in Visual Basic).
		/// </exception>
		/// <PermissionSet>
		/// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
		/// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
		/// </PermissionSet>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info?.AddValue("RC", ReturnCode);
			base.GetObjectData(info, context);
		}
	}
	/// <summary>
	/// Basic class for interfacing with TWAIN. You should only have one of this per application process.
	/// </summary>
	public class TwainSession : ITwainSessionInternal, ITwainSession, IEnumerable<DataSource>, IEnumerable, INotifyPropertyChanged, ITripletControl, MessageLoopHook.IWinMessageFilter
	{
		private object _callbackObj;

		private TWIdentity _appId;

		private TWUserInterface _twui;

		private readonly Dictionary<string, DataSource> _ownedSources;

		private DGCustom _dgCustom;

		private int _state = 1;

		private MessageLoopHook _msgLoopHook;

		private bool _closeRequested;

		private DGAudio _dgAudio;

		private DGControl _dgControl;

		private DGImage _dgImage;

		private bool _disabling;

		DGCustom ITripletControl.DGCustom => DGCustom;

		public DGCustom DGCustom
		{
			get
			{
				if (_dgCustom == null)
				{
					_dgCustom = new DGCustom(this);
				}
				return _dgCustom;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether calls to triplets will verify the current twain session state.
		/// </summary>
		/// <value>
		///   <c>true</c> if state value is enforced; otherwise, <c>false</c>.
		/// </value>
		public bool EnforceState { get; set; }

		/// <summary>
		/// [Experimental] Gets or sets the optional synchronization context when not specifying a <see cref="T:NTwain.MessageLoopHook" /> on <see cref="M:NTwain.TwainSession.Open" />.
		/// This allows events to be raised on the thread associated with the context. This is experimental is not recommended for use.
		/// </summary>
		/// <value>
		/// The synchronization context.
		/// </value>
		public SynchronizationContext SynchronizationContext { get; set; }

		/// <summary>
		/// Gets the currently open source.
		/// </summary>
		/// <value>
		/// The current source.
		/// </value>
		public DataSource CurrentSource { get; private set; }

		/// <summary>
		/// Gets or sets the default source for this application.
		/// While this can be get as long as the session is open,
		/// it can only be set at State 3.
		/// </summary>
		/// <value>
		/// The default source.
		/// </value>
		public DataSource DefaultSource
		{
			get
			{
				if (((ITripletControl)this).DGControl.Identity.GetDefault(out var source) == ReturnCode.Success)
				{
					return GetSourceInstance(this, source);
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					((ITripletControl)this).DGControl.Identity.Set(value.Identity);
				}
			}
		}

		/// <summary>
		/// Gets the current state number as defined by the TWAIN spec.
		/// </summary>
		/// <value>
		/// The state.
		/// </value>
		public int State
		{
			get
			{
				return _state;
			}
			private set
			{
				if (value > 0 && value < 8)
				{
					_state = value;
					OnPropertyChanged("State");
					SafeAsyncSyncableRaiseOnEvent(OnStateChanged, StateChanged);
				}
			}
		}

		/// <summary>
		/// Gets the named state value as defined by the TWAIN spec.
		/// </summary>
		/// <value>
		/// The state.
		/// </value>
		public State StateEx => (State)_state;

		/// <summary>
		/// Quick flag to check if the DSM has been opened.
		/// </summary>
		public bool IsDsmOpen => State > 2;

		/// <summary>
		/// Quick flag to check if a source has been opened.
		/// </summary>
		public bool IsSourceOpen => State > 3;

		/// <summary>
		/// Quick flag to check if a source has been enabled.
		/// </summary>
		public bool IsSourceEnabled => State > 4;

		/// <summary>
		/// Quick flag to check if a source is in the transferring state.
		/// </summary>
		public bool IsTransferring => State > 5;

		/// <summary>
		/// Whether to stop the transfer process when transfer error is encountered.
		/// May be required on some sources.
		/// </summary>
		/// <value>
		/// <c>true</c> to stop on transfer error; otherwise, <c>false</c>.
		/// </value>
		public bool StopOnTransferError { get; set; }

		/// <summary>
		/// Gets the reason a source was disabled (dropped from state 5) if it's due to user action.
		/// Mostly only <see cref="F:NTwain.Data.Message.CloseDSOK" /> or <see cref="F:NTwain.Data.Message.CloseDSReq" />.
		/// </summary>
		/// <value>
		/// The dialog result.
		/// </value>
		public NTwain.Data.Message DisableReason { get; private set; }

		MessageLoopHook ITwainSessionInternal.MessageLoopHook => _msgLoopHook;

		/// <summary>
		/// Gets the app id used for the session.
		/// </summary>
		/// <value>The app id.</value>
		TWIdentity ITwainSessionInternal.AppId => _appId;

		bool ITwainSessionInternal.CloseDSRequested => _closeRequested;

		DGAudio ITwainSessionInternal.DGAudio
		{
			get
			{
				if (_dgAudio == null)
				{
					_dgAudio = new DGAudio(this);
				}
				return _dgAudio;
			}
		}

		DGControl ITripletControl.DGControl => DGControl;

		protected DGControl DGControl
		{
			get
			{
				if (_dgControl == null)
				{
					_dgControl = new DGControl(this);
				}
				return _dgControl;
			}
		}

		DGImage ITripletControl.DGImage => DGImage;

		protected DGImage DGImage
		{
			get
			{
				if (_dgImage == null)
				{
					_dgImage = new DGImage(this);
				}
				return _dgImage;
			}
		}

		/// <summary>
		/// Occurs when <see cref="P:NTwain.TwainSession.State" /> has changed.
		/// </summary>
		public event EventHandler StateChanged;

		/// <summary>
		/// Occurs when <see cref="P:NTwain.TwainSession.CurrentSource" /> has changed.
		/// </summary>
		public event EventHandler SourceChanged;

		/// <summary>
		/// Occurs when source has been disabled (back to state 4).
		/// </summary>
		public event EventHandler SourceDisabled;

		/// <summary>
		/// Occurs when the source has generated an event.
		/// </summary>
		public event EventHandler<DeviceEventArgs> DeviceEvent;

		/// <summary>
		/// Occurs when a data transfer is ready.
		/// </summary>
		public event EventHandler<TransferReadyEventArgs> TransferReady;

		/// <summary>
		/// Occurs when data has been transferred.
		/// </summary>
		public event EventHandler<DataTransferredEventArgs> DataTransferred;

		/// <summary>
		/// Occurs when a transfer was canceled.
		/// </summary>
		public event EventHandler<TransferCanceledEventArgs> TransferCanceled;

		/// <summary>
		/// Occurs when an error has been encountered during transfer.
		/// </summary>
		public event EventHandler<TransferErrorEventArgs> TransferError;

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TwainSession" /> class.
		/// </summary>
		/// <param name="supportedGroups">The supported groups.</param>
		public TwainSession(DataGroups supportedGroups)
			: this(TWIdentity.CreateFromAssembly(supportedGroups, Assembly.GetEntryAssembly()))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TwainSession" /> class.
		/// </summary>
		/// <param name="appId">The app id that represents calling application.</param>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		public TwainSession(TWIdentity appId)
		{
			if (appId == null)
			{
				throw new ArgumentNullException("appId");
			}
			_appId = appId;
			_ownedSources = new Dictionary<string, DataSource>();
			if (PlatformInfo.Current.IsSupported)
			{
				((ITwainSessionInternal)this).ChangeState(2, false);
			}
		}

		private DataSource GetSourceInstance(ITwainSessionInternal session, TWIdentity sourceId)
		{
			DataSource dataSource = null;
			PlatformInfo.Current.Log.Debug("Source id = {0}", sourceId.Id);
			string key = string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}|{3}", sourceId.Id, sourceId.Manufacturer, sourceId.ProductFamily, sourceId.ProductName);
			return _ownedSources.ContainsKey(key) ? _ownedSources[key] : (_ownedSources[key] = new DataSource(session, sourceId));
		}

		/// <summary>
		/// Try to show the built-in source selector dialog and return the selected source.
		/// This is not recommended and is only included for completeness.
		/// </summary>
		/// <returns></returns>
		public DataSource ShowSourceSelector()
		{
			if (((ITripletControl)this).DGControl.Identity.UserSelect(out var source) == ReturnCode.Success)
			{
				return GetSourceInstance(this, source);
			}
			return null;
		}

		/// <summary>
		/// Opens the data source manager. This must be the first method used
		/// before using other TWAIN functions. Calls to this must be followed by
		/// <see cref="M:NTwain.TwainSession.Close" /> when done with a TWAIN session.
		/// </summary>
		/// <returns></returns>
		public ReturnCode Open()
		{
			return Open(new InternalMessageLoopHook());
		}

		/// <summary>
		/// Opens the data source manager. This must be the first method used
		/// before using other TWAIN functions. Calls to this must be followed by
		/// <see cref="M:NTwain.TwainSession.Close" /> when done with a TWAIN session.
		/// </summary>
		/// <param name="messageLoopHook">The message loop hook.</param>
		/// <returns></returns>
		/// <exception cref="T:System.ArgumentNullException">messageLoopHook</exception>
		public ReturnCode Open(MessageLoopHook messageLoopHook)
		{
			if (messageLoopHook == null)
			{
				throw new ArgumentNullException("messageLoopHook");
			}
			_msgLoopHook = messageLoopHook;
			_msgLoopHook.Start(this);
			ReturnCode rc = ReturnCode.Failure;
			_msgLoopHook.Invoke(() =>
			{
				PlatformInfo.Current.Log.Debug("Thread {0}: OpenManager.", Thread.CurrentThread.ManagedThreadId);
				rc = ((ITripletControl)this).DGControl.Parent.OpenDsm(_msgLoopHook.Handle);
				if (rc == ReturnCode.Success && (_appId.DataFunctionalities & DataFunctionalities.Dsm2) == DataFunctionalities.Dsm2)
				{
					rc = ((ITripletControl)this).DGControl.EntryPoint.Get(out var entryPoint);
					if (rc == ReturnCode.Success)
					{
						PlatformInfo.InternalCurrent.MemoryManager = entryPoint;
						PlatformInfo.Current.Log.Debug("Using TWAIN2 memory functions.");
					}
					else
					{
						Close();
					}
				}
			});
			return rc;
		}

		/// <summary>
		/// Closes the data source manager.
		/// </summary>
		/// <returns></returns>
		public ReturnCode Close()
		{
			ReturnCode rc = ReturnCode.Failure;
			_msgLoopHook?.Invoke(() =>
			{
				PlatformInfo.Current.Log.Debug("Thread {0}: CloseManager.", Thread.CurrentThread.ManagedThreadId);
				rc = ((ITripletControl)this).DGControl.Parent.CloseDsm(_msgLoopHook.Handle);
				if (rc == ReturnCode.Success)
				{
					PlatformInfo.InternalCurrent.MemoryManager = null;
					_msgLoopHook.Stop();
					_msgLoopHook = null;
				}
			});
			return rc;
		}

		/// <summary>
		/// Gets list of sources available in the system.
		/// Only call this at state 2 or higher.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<DataSource> GetSources()
		{
			return this;
		}

		/// <summary>
		/// Quick shortcut to open a source.
		/// </summary>
		/// <param name="sourceName">Name of the source.</param>
		/// <returns></returns>
		public ReturnCode OpenSource(string sourceName)
		{
			_ = CurrentSource;
			return this.Where((DataSource s) => string.Equals(s.Name, sourceName)).FirstOrDefault()?.Open() ?? ReturnCode.Failure;
		}

		/// <summary>
		/// Quick shortcut to open a source.
		/// </summary>
		/// <param name="sourceId">Id of the source.</param>
		/// <returns></returns>
		public ReturnCode OpenSource(int sourceId)
		{
			_ = CurrentSource;
			return this.Where((DataSource s) => s.Id == sourceId).FirstOrDefault()?.Open() ?? ReturnCode.Failure;
		}

		/// <summary>
		/// Gets the manager status. Only call this at state 2 or higher.
		/// </summary>
		/// <returns></returns>
		public TWStatus GetStatus()
		{
			((ITripletControl)this).DGControl.Status.GetManager(out var status);
			return status;
		}

		/// <summary>
		/// Gets the manager status string. Only call this at state 3 or higher.
		/// </summary>
		/// <param name="status">Status from previous calls.</param>
		/// <returns></returns>
		public TWStatusUtf8 GetStatusUtf8(TWStatus status)
		{
			((ITripletControl)this).DGControl.StatusUtf8.GetManager(status, out var utf8status);
			return utf8status;
		}

		/// <summary>
		/// Forces the stepping down of an opened source when things gets out of control.
		/// Used when session state and source state become out of sync.
		/// </summary>
		/// <param name="targetState">State of the target.</param>
		public void ForceStepDown(int targetState)
		{
			PlatformInfo.Current.Log.Debug("Thread {0}: ForceStepDown.", Thread.CurrentThread.ManagedThreadId);
			bool enforceState = EnforceState;
			EnforceState = false;
			_msgLoopHook?.Invoke(() =>
			{
				if (targetState < 7 && CurrentSource != null)
				{
					TWPendingXfers tWPendingXfers = new TWPendingXfers();
					GCHandle gCHandle = GCHandle.Alloc(tWPendingXfers, GCHandleType.Pinned);
					try
					{
						((ITripletControl)this).DGControl.PendingXfers.EndXfer(tWPendingXfers);
					}
					finally
					{
						gCHandle.Free();
					}
				}
				if (targetState < 6 && CurrentSource != null)
				{
					TWPendingXfers tWPendingXfers2 = new TWPendingXfers();
					GCHandle gCHandle2 = GCHandle.Alloc(tWPendingXfers2, GCHandleType.Pinned);
					try
					{
						((ITripletControl)this).DGControl.PendingXfers.Reset(tWPendingXfers2);
					}
					finally
					{
						gCHandle2.Free();
					}
				}
				if (targetState < 5 && CurrentSource != null)
				{
					((ITwainSessionInternal)this).DisableSource();
				}
				if (targetState < 4 && CurrentSource != null)
				{
					CurrentSource.Close();
				}
				if (targetState < 3)
				{
					Close();
				}
			});
			EnforceState = enforceState;
		}

		/// <summary>
		/// Raises the <see cref="E:NTwain.TwainSession.PropertyChanged" /> event.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected void OnPropertyChanged(string propertyName)
		{
			SynchronizationContext synchronizationContext = SynchronizationContext;
			if (synchronizationContext == null)
			{
				try
				{
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
					return;
				}
				catch (Exception exception)
				{
					PlatformInfo.Current.Log.Error("PropertyChanged event error.", exception);
					return;
				}
			}
			synchronizationContext.Post((object o) =>
			{
				try
				{
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
				}
				catch (Exception exception2)
				{
					PlatformInfo.Current.Log.Error("PropertyChanged event error.", exception2);
				}
			}, null);
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<DataSource> GetEnumerator()
		{
			TWIdentity source;
			for (ReturnCode returnCode = ((ITripletControl)this).DGControl.Identity.GetFirst(out source); returnCode == ReturnCode.Success; returnCode = ((ITripletControl)this).DGControl.Identity.GetNext(out source))
			{
				yield return GetSourceInstance(this, source);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Raises event and if applicable marshal it asynchronously to the <see cref="P:NTwain.TwainSession.SynchronizationContext" /> thread
		/// without exceptions.
		/// </summary>
		/// <param name="onEventFunc">The on event function.</param>
		/// <param name="handler">The handler.</param>
		private void SafeAsyncSyncableRaiseOnEvent(Action onEventFunc, EventHandler handler)
		{
			SynchronizationContext synchronizationContext = SynchronizationContext;
			if (synchronizationContext == null)
			{
				try
				{
					onEventFunc();
					if (handler != null)
					{
						handler(this, EventArgs.Empty);
					}
					return;
				}
				catch (Exception exception)
				{
					PlatformInfo.Current.Log.Error("{0} event error.", exception, handler.Method.Name);
					return;
				}
			}
			synchronizationContext.Post((object o) =>
			{
				try
				{
					onEventFunc();
					if (handler != null)
					{
						handler(this, EventArgs.Empty);
					}
				}
				catch (Exception exception2)
				{
					PlatformInfo.Current.Log.Error("{0} event error.", exception2, handler.Method.Name);
				}
			}, null);
		}

		/// <summary>
		/// Raises event and if applicable marshal it synchronously to the <see cref="P:NTwain.TwainSession.SynchronizationContext" /> thread
		/// without exceptions.
		/// </summary>
		/// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
		/// <param name="onEventFunc">The on event function.</param>
		/// <param name="handler">The handler.</param>
		/// <param name="e">The TEventArgs instance containing the event data.</param>
		private void SafeSyncableRaiseOnEvent<TEventArgs>(Action<TEventArgs> onEventFunc, EventHandler<TEventArgs> handler, TEventArgs e) where TEventArgs : EventArgs
		{
			SynchronizationContext synchronizationContext = SynchronizationContext;
			if (synchronizationContext == null)
			{
				PlatformInfo.Current.Log.Debug("Trying to raise event {0} on thread {1} without sync.", e.GetType().Name, Thread.CurrentThread.ManagedThreadId);
				try
				{
					onEventFunc(e);
					if (handler != null)
					{
						handler(this, e);
					}
					return;
				}
				catch (Exception exception)
				{
					PlatformInfo.Current.Log.Error("{0} event error.", exception, handler.Method.Name);
					return;
				}
			}
			PlatformInfo.Current.Log.Debug("Trying to raise event {0} on thread {1} with sync.", e.GetType().Name, Thread.CurrentThread.ManagedThreadId);
			synchronizationContext.Send((object o) =>
			{
				try
				{
					onEventFunc(e);
					if (handler != null)
					{
						handler(this, e);
					}
				}
				catch (Exception exception2)
				{
					PlatformInfo.Current.Log.Error("{0} event error.", exception2, handler.Method.Name);
				}
			}, null);
		}

		/// <summary>
		/// Called when <see cref="P:NTwain.TwainSession.State" /> changed.
		/// </summary>
		protected virtual void OnStateChanged()
		{
		}

		/// <summary>
		/// Called when <see cref="P:NTwain.TwainSession.CurrentSource" /> changed.
		/// </summary>
		protected virtual void OnSourceChanged()
		{
		}

		/// <summary>
		/// Called when source has been disabled (back to state 4).
		/// </summary>
		protected virtual void OnSourceDisabled()
		{
		}

		/// <summary>
		/// Called when the source has generated an event.
		/// </summary>
		/// <param name="e">The <see cref="T:NTwain.DeviceEventArgs" /> instance containing the event data.</param>
		protected virtual void OnDeviceEvent(DeviceEventArgs e)
		{
		}

		/// <summary>
		/// Called when a data transfer is ready.
		/// </summary>
		/// <param name="e">The <see cref="T:NTwain.TransferReadyEventArgs" /> instance containing the event data.</param>
		protected virtual void OnTransferReady(TransferReadyEventArgs e)
		{
		}

		/// <summary>
		/// Called when data has been transferred.
		/// </summary>
		/// <param name="e">The <see cref="T:NTwain.DataTransferredEventArgs" /> instance containing the event data.</param>
		protected virtual void OnDataTransferred(DataTransferredEventArgs e)
		{
		}

		/// <summary>
		/// Called when a transfer was canceled.
		/// </summary>
		/// <param name="e">The <see cref="T:NTwain.TransferCanceledEventArgs" /> instance containing the event data.</param>
		protected virtual void OnTransferCanceled(TransferCanceledEventArgs e)
		{
		}

		/// <summary>
		/// Called when an error has been encountered during transfer.
		/// </summary>
		/// <param name="e">The <see cref="T:NTwain.TransferErrorEventArgs" /> instance containing the event data.</param>
		protected virtual void OnTransferError(TransferErrorEventArgs e)
		{
		}

		void ITwainSessionInternal.UpdateCallback()
		{
			if (State < 4)
			{
				_callbackObj = null;
				return;
			}
			ReturnCode returnCode = ReturnCode.Failure;
			if (_appId.ProtocolMajor > 2 || (_appId.ProtocolMajor >= 2 && _appId.ProtocolMinor >= 2))
			{
				TWCallback2 tWCallback = new TWCallback2(HandleCallback);
				returnCode = ((ITripletControl)this).DGControl.Callback2.RegisterCallback(tWCallback);
				if (returnCode == ReturnCode.Success)
				{
					PlatformInfo.Current.Log.Debug("Registered callback2 OK.");
					_callbackObj = tWCallback;
				}
			}
			if (returnCode != ReturnCode.Success)
			{
				TWCallback tWCallback2 = new TWCallback(HandleCallback);
				if (((ITripletControl)this).DGControl.Callback.RegisterCallback(tWCallback2) == ReturnCode.Success)
				{
					PlatformInfo.Current.Log.Debug("Registered callback OK.");
					_callbackObj = tWCallback2;
				}
			}
		}

		void ITwainSessionInternal.ChangeState(int newState, bool notifyChange)
		{
			_state = newState;
			if (notifyChange)
			{
				OnPropertyChanged("State");
				SafeAsyncSyncableRaiseOnEvent(OnStateChanged, StateChanged);
			}
		}

		ICommittable ITwainSessionInternal.GetPendingStateChanger(int newState)
		{
			return new TentativeStateCommitable(this, newState);
		}

		void ITwainSessionInternal.ChangeCurrentSource(DataSource source)
		{
			CurrentSource = source;
			DisableReason = NTwain.Data.Message.Null;
			OnPropertyChanged("CurrentSource");
			SafeAsyncSyncableRaiseOnEvent(OnSourceChanged, SourceChanged);
		}

		void ITwainSessionInternal.SafeSyncableRaiseEvent(DataTransferredEventArgs e)
		{
			SafeSyncableRaiseOnEvent(OnDataTransferred, DataTransferred, e);
		}

		void ITwainSessionInternal.SafeSyncableRaiseEvent(TransferCanceledEventArgs e)
		{
			SafeSyncableRaiseOnEvent(OnTransferCanceled, TransferCanceled, e);
		}

		void ITwainSessionInternal.SafeSyncableRaiseEvent(TransferErrorEventArgs e)
		{
			SafeSyncableRaiseOnEvent(OnTransferError, TransferError, e);
		}

		void ITwainSessionInternal.SafeSyncableRaiseEvent(TransferReadyEventArgs e)
		{
			SafeSyncableRaiseOnEvent(OnTransferReady, TransferReady, e);
		}

		/// <summary>
		/// Enables the source to start transferring.
		/// </summary>
		/// <param name="mode">The mode.</param>
		/// <param name="modal">if set to <c>true</c> any driver UI will display as modal.</param>
		/// <param name="windowHandle">The window handle if modal.</param>
		/// <returns></returns>
		ReturnCode ITwainSessionInternal.EnableSource(SourceEnableMode mode, bool modal, IntPtr windowHandle)
		{
			_closeRequested = false;
			DisableReason = NTwain.Data.Message.Null;
			ReturnCode rc = ReturnCode.Failure;
			_msgLoopHook?.Invoke(() =>
			{
				PlatformInfo.Current.Log.Debug("Thread {0}: EnableSource with {1}.", Thread.CurrentThread.ManagedThreadId, mode);
				_twui = new TWUserInterface();
				_twui.ShowUI = mode == SourceEnableMode.ShowUI;
				_twui.ModalUI = modal;
				_twui.hParent = windowHandle;
				if (mode == SourceEnableMode.ShowUIOnly)
				{
					rc = ((ITripletControl)this).DGControl.UserInterface.EnableDSUIOnly(_twui);
				}
				else
				{
					rc = ((ITripletControl)this).DGControl.UserInterface.EnableDS(_twui);
				}
			});
			return rc;
		}

		ReturnCode ITwainSessionInternal.DisableSource()
		{
			ReturnCode rc = ReturnCode.Failure;
			if (!_disabling)
			{
				_disabling = true;
				try
				{
					_msgLoopHook?.Invoke(() =>
					{
						PlatformInfo.Current.Log.Debug("Thread {0}: DisableSource.", Thread.CurrentThread.ManagedThreadId);
						rc = ((ITripletControl)this).DGControl.UserInterface.DisableDS(_twui);
						if (rc == ReturnCode.Success)
						{
							SafeAsyncSyncableRaiseOnEvent(OnSourceDisabled, SourceDisabled);
						}
					});
				}
				finally
				{
					_disabling = false;
				}
			}
			return rc;
		}

		/// <summary>
		/// Checks and handles the message if it's a TWAIN message.
		/// </summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="msg">The message.</param>
		/// <param name="wParam">The w parameter.</param>
		/// <param name="lParam">The l parameter.</param>
		/// <returns>
		/// true if handled internally.
		/// </returns>
		public bool IsTwainMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
		{
			bool result = false;
			if (_state >= 5)
			{
				IntPtr intPtr = IntPtr.Zero;
				try
				{
					MESSAGE structure = new MESSAGE(hwnd, msg, wParam, lParam);
					intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
					Marshal.StructureToPtr(structure, intPtr, fDeleteOld: false);
					TWEvent tWEvent = new TWEvent();
					tWEvent.pEvent = intPtr;
					if (result = ((ITripletControl)this).DGControl.Event.ProcessEvent(tWEvent) == ReturnCode.DSEvent)
					{
						PlatformInfo.Current.Log.Debug("Thread {0}: HandleWndProcMessage at state {1} with MSG={2}.", Thread.CurrentThread.ManagedThreadId, State, tWEvent.TWMessage);
						HandleSourceMsg(tWEvent.TWMessage);
					}
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
			}
			return result;
		}

		private ReturnCode HandleCallback(TWIdentity origin, TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, IntPtr data)
		{
			if (origin != null && CurrentSource != null && origin.Id == CurrentSource.Identity.Id && _state >= 5)
			{
				PlatformInfo.Current.Log.Debug("Thread {0}: CallbackHandler at state {1} with MSG={2}.", Thread.CurrentThread.ManagedThreadId, State, msg);
				if (origin.ProductName.IndexOf("scanjet", StringComparison.OrdinalIgnoreCase) > -1)
				{
					_msgLoopHook?.Invoke(() =>
					{
						HandleSourceMsg(msg);
					});
				}
				else
				{
					_msgLoopHook?.BeginInvoke(() =>
					{
						HandleSourceMsg(msg);
					});
				}
				return ReturnCode.Success;
			}
			return ReturnCode.Failure;
		}

		private void HandleSourceMsg(NTwain.Data.Message msg)
		{
			PlatformInfo.Current.Log.Debug("Got TWAIN msg " + msg);
			switch (msg)
			{
			case NTwain.Data.Message.XferReady:
				if (State < 6)
				{
					State = 6;
				}
				TransferLogic.DoTransferRoutine(this);
				break;
			case NTwain.Data.Message.DeviceEvent:
			{
				if (((ITripletControl)this).DGControl.DeviceEvent.Get(out var sourceDeviceEvent) == ReturnCode.Success)
				{
					SafeSyncableRaiseOnEvent(OnDeviceEvent, DeviceEvent, new DeviceEventArgs(sourceDeviceEvent));
				}
				break;
			}
			case NTwain.Data.Message.CloseDSReq:
			case NTwain.Data.Message.CloseDSOK:
				DisableReason = msg;
				if (State > 5)
				{
					_closeRequested = true;
				}
				else if (State == 5)
				{
					((ITwainSessionInternal)this).DisableSource();
				}
				break;
			}
		}
	}
	/// <summary>
	/// Represents an exception from calling a TWAIN triplet operation in the wrong state.
	/// </summary>
	[Serializable]
	public class TwainStateException : TwainException
	{
		/// <summary>
		/// Gets the allowed minimum state.
		/// </summary>
		/// <value>The minimum.</value>
		public int MinStateExpected { get; private set; }

		/// <summary>
		/// Gets the allowed maximum state.
		/// </summary>
		/// <value>The maximum.</value>
		public int MaxStateExpected { get; private set; }

		/// <summary>
		/// Gets the actual state number.
		/// </summary>
		/// <value>The state.</value>
		public int ActualState { get; private set; }

		/// <summary>
		/// Gets the triplet data group.
		/// </summary>
		/// <value>The data group.</value>
		public DataGroups DataGroup { get; private set; }

		/// <summary>
		/// Gets the triplet data argument type.
		/// </summary>
		/// <value>The type of the argument.</value>
		public DataArgumentType ArgumentType { get; private set; }

		/// <summary>
		/// Gets the triplet message.
		/// </summary>
		/// <value>The twain message.</value>
		public NTwain.Data.Message TwainMessage { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TwainStateException" /> class.
		/// </summary>
		public TwainStateException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TwainStateException" /> class.
		/// </summary>
		/// <param name="currentState">The actual state number.</param>
		/// <param name="minStateExpected">The minimum state allowed.</param>
		/// <param name="maxStateExpected">The maximum state allowed.</param>
		/// <param name="dataGroup">The data group used.</param>
		/// <param name="argumentType">The data argument type used.</param>
		/// <param name="twainMessage">The twain message used.</param>
		/// <param name="message">The message.</param>
		public TwainStateException(int currentState, int minStateExpected, int maxStateExpected, DataGroups dataGroup, DataArgumentType argumentType, NTwain.Data.Message twainMessage, string message)
			: base(ReturnCode.Success, message)
		{
			ActualState = currentState;
			MinStateExpected = minStateExpected;
			MaxStateExpected = maxStateExpected;
			DataGroup = dataGroup;
			ArgumentType = argumentType;
			TwainMessage = twainMessage;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.TwainStateException" /> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="info" /> parameter is null.
		/// </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">
		/// The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0).
		/// </exception>
		protected TwainStateException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info != null)
			{
				MinStateExpected = info.GetInt32("MIN");
				MaxStateExpected = info.GetInt32("MAX");
				ActualState = info.GetInt32("State");
				DataGroup = (DataGroups)info.GetUInt32("DG");
				ArgumentType = (DataArgumentType)info.GetUInt16("DAT");
				TwainMessage = (NTwain.Data.Message)info.GetUInt16("MSG");
			}
		}

		/// <summary>
		/// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="info" /> parameter is a null reference (Nothing in Visual Basic).
		/// </exception>
		/// <PermissionSet>
		/// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
		/// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
		/// </PermissionSet>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				info.AddValue("MIN", MinStateExpected);
				info.AddValue("MAX", MaxStateExpected);
				info.AddValue("State", ActualState);
				info.AddValue("DG", DataGroup);
				info.AddValue("DAT", ArgumentType);
				info.AddValue("MSG", TwainMessage);
			}
			base.GetObjectData(info, context);
		}
	}
	/// <summary>
	/// Contains extension methods for wpf images.
	/// </summary>
	public static class WpfImageTools
	{
		/// <summary>
		/// Loads a <see cref="T:System.IO.Stream" /> into WPF <see cref="T:System.Windows.Media.Imaging.BitmapSource" />. The image created
		/// will be a copy so the stream can be disposed once this call returns.
		/// </summary>
		/// <param name="stream">The image stream.</param>
		/// <returns></returns>
		public static BitmapSource ConvertToWpfBitmap(this Stream stream)
		{
			return stream.ConvertToWpfBitmap(0, 0);
		}

		/// <summary>
		/// Loads a <see cref="T:System.IO.Stream" /> into WPF <see cref="T:System.Windows.Media.Imaging.BitmapSource" />. The image created
		/// will be a copy so the stream can be disposed once this call returns.
		/// </summary>
		/// <param name="stream">The image stream.</param>
		/// <param name="decodeWidth">Max width of the decoded image. Pass 0 to use default.</param>
		/// <param name="decodeHeight">Max height of the decoded image. Pass 0 to use default.</param>
		/// <returns></returns>
		public static BitmapSource ConvertToWpfBitmap(this Stream stream, int decodeWidth, int decodeHeight)
		{
			if (stream != null)
			{
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.DecodePixelHeight = decodeHeight;
				bitmapImage.DecodePixelWidth = decodeWidth;
				bitmapImage.StreamSource = stream;
				bitmapImage.EndInit();
				if (bitmapImage.CanFreeze)
				{
					bitmapImage.Freeze();
				}
				return bitmapImage;
			}
			return null;
		}
	}
}
namespace NTwain.Triplets
{
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.AudioFileXfer" />.
	/// </summary>
	internal sealed class AudioFileXfer : TripletBase
	{
		internal AudioFileXfer(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This operation is used to initiate the transfer of audio from the Source to the application via the
		/// disk-file transfer mechanism. It causes the transfer to begin.
		/// </summary>
		/// <returns></returns>
		public ReturnCode Get()
		{
			Session.VerifyState(6, 6, DataGroups.Audio, DataArgumentType.AudioFileXfer, NTwain.Data.Message.Get);
			IntPtr data = IntPtr.Zero;
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataGroups.Audio, DataArgumentType.AudioFileXfer, NTwain.Data.Message.Get, ref data);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.AudioInfo" />.
	/// </summary>
	internal sealed class AudioInfo : TripletBase
	{
		internal AudioInfo(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Used to get the information of the current audio data ready to transfer.
		/// </summary>
		/// <param name="info">The info.</param>
		/// <returns></returns>
		public ReturnCode Get(out TWAudioInfo info)
		{
			Session.VerifyState(6, 7, DataGroups.Audio, DataArgumentType.AudioInfo, NTwain.Data.Message.Get);
			info = new TWAudioInfo();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, info);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.AudioNativeXfer" />.
	/// </summary>
	internal sealed class AudioNativeXfer : TripletBase
	{
		internal AudioNativeXfer(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Causes the transfer of an audio data from the Source to the application, via the Native
		/// transfer mechanism, to begin. The resulting data is stored in main memory in a single block.
		/// The data is stored in AIFF format on the Macintosh and as a WAV format under Microsoft
		/// Windows. The size of the audio snippet that can be transferred is limited to the size of the
		/// memory block that can be allocated by the Source.
		/// </summary>
		/// <param name="handle">The handle.</param>
		/// <returns></returns>
		public ReturnCode Get(ref IntPtr handle)
		{
			Session.VerifyState(6, 6, DataGroups.Audio, DataArgumentType.AudioNativeXfer, NTwain.Data.Message.Get);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataGroups.Audio, DataArgumentType.AudioNativeXfer, NTwain.Data.Message.Get, ref handle);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataGroups.Audio" />.
	/// </summary>
	internal sealed class DGAudio
	{
		private ITwainSessionInternal _session;

		private AudioFileXfer _audioFileXfer;

		private AudioInfo _audioInfo;

		private AudioNativeXfer _audioNativeXfer;

		internal AudioFileXfer AudioFileXfer
		{
			get
			{
				if (_audioFileXfer == null)
				{
					_audioFileXfer = new AudioFileXfer(_session);
				}
				return _audioFileXfer;
			}
		}

		internal AudioInfo AudioInfo
		{
			get
			{
				if (_audioInfo == null)
				{
					_audioInfo = new AudioInfo(_session);
				}
				return _audioInfo;
			}
		}

		internal AudioNativeXfer AudioNativeXfer
		{
			get
			{
				if (_audioNativeXfer == null)
				{
					_audioNativeXfer = new AudioNativeXfer(_session);
				}
				return _audioNativeXfer;
			}
		}

		internal DGAudio(ITwainSessionInternal session)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			_session = session;
		}
	}
	internal sealed class Callback : TripletBase
	{
		internal Callback(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This triplet is sent to the DSM by the Application to register the application’s entry point with
		/// the DSM, so that the DSM can use callbacks to inform the application of events generated by the
		/// DS.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <returns></returns>
		public ReturnCode RegisterCallback(TWCallback callback)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.Callback, NTwain.Data.Message.RegisterCallback);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.RegisterCallback, callback);
		}
	}
	internal sealed class Callback2 : TripletBase
	{
		internal Callback2(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This triplet is sent to the DSM by the Application to register the application’s entry point with
		/// the DSM, so that the DSM can use callbacks to inform the application of events generated by the
		/// DS.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <returns></returns>
		public ReturnCode RegisterCallback(TWCallback2 callback)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.Callback, NTwain.Data.Message.RegisterCallback);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.RegisterCallback, callback);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.Capability" />.
	/// </summary>
	public sealed class Capability : TripletBase
	{
		internal Capability(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Returns the Source’s Current, Default and Available Values for a specified capability.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode Get(TWCapability capability)
		{
			Session.VerifyState(4, 7, DataGroups.Control, DataArgumentType.Capability, NTwain.Data.Message.Get);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataArgumentType.Capability, NTwain.Data.Message.Get, capability);
		}

		/// <summary>
		/// Returns the Source’s Current Value for the specified capability.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode GetCurrent(TWCapability capability)
		{
			Session.VerifyState(4, 7, DataGroups.Control, DataArgumentType.Capability, NTwain.Data.Message.GetCurrent);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataArgumentType.Capability, NTwain.Data.Message.GetCurrent, capability);
		}

		/// <summary>
		/// Returns the Source’s Default Value. This is the Source’s preferred default value.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode GetDefault(TWCapability capability)
		{
			Session.VerifyState(4, 7, DataGroups.Control, DataArgumentType.Capability, NTwain.Data.Message.GetDefault);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataArgumentType.Capability, NTwain.Data.Message.GetDefault, capability);
		}

		/// <summary>
		/// Returns help text suitable for use in a GUI; for instance: "Specify the amount of detail in an
		/// image. Higher values result in more detail." for ICapXRESOLUTION.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode GetHelp(TWCapability capability)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.Capability, NTwain.Data.Message.GetHelp);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataArgumentType.Capability, NTwain.Data.Message.GetHelp, capability);
		}

		/// <summary>
		/// Returns a label suitable for use in a GUI, for instance "Resolution:"
		/// for ICapXRESOLUTION.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode GetLabel(TWCapability capability)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.Capability, NTwain.Data.Message.GetLabel);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataArgumentType.Capability, NTwain.Data.Message.GetLabel, capability);
		}

		/// <summary>
		/// Return all of the labels for a capability of type <see cref="T:NTwain.Data.TWArray" /> or <see cref="T:NTwain.Data.TWEnumeration" />, for example
		/// "US Letter" for ICapSupportedSizes’ TWSS_USLETTER.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode GetLabelEnum(TWCapability capability)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.Capability, NTwain.Data.Message.GetLabelEnum);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataArgumentType.Capability, NTwain.Data.Message.GetLabelEnum, capability);
		}

		/// <summary>
		/// Returns the Source’s support status of this capability.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode QuerySupport(TWCapability capability)
		{
			Session.VerifyState(4, 7, DataGroups.Control, DataArgumentType.Capability, NTwain.Data.Message.QuerySupport);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataArgumentType.Capability, NTwain.Data.Message.QuerySupport, capability);
		}

		/// <summary>
		/// Change the Current Value of the specified capability back to its power-on value and return the
		/// new Current Value.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode Reset(TWCapability capability)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.Capability, NTwain.Data.Message.Reset);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataArgumentType.Capability, NTwain.Data.Message.Reset, capability);
		}

		/// <summary>
		/// This command resets all of the current values and constraints to their defaults for all of the
		/// negotiable capabilities supported by the driver.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode ResetAll(TWCapability capability)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.Capability, NTwain.Data.Message.ResetAll);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataArgumentType.Capability, NTwain.Data.Message.ResetAll, capability);
		}

		/// <summary>
		/// Changes the Current Value(s) and Available Values of the specified capability to those specified
		/// by the application. As of TWAIN 2.2 this only modifies the Current Value of the specified capability, constraints cannot be
		/// changed with this.
		/// Current Values are set when the container is a <see cref="T:NTwain.Data.TWOneValue" /> or <see cref="T:NTwain.Data.TWArray" />. Available and
		/// Current Values are set when the container is a <see cref="T:NTwain.Data.TWEnumeration" /> or <see cref="T:NTwain.Data.TWRange" />.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode Set(TWCapability capability)
		{
			Session.VerifyState(4, 6, DataGroups.Control, DataArgumentType.Capability, NTwain.Data.Message.Set);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataArgumentType.Capability, NTwain.Data.Message.Set, capability);
		}

		/// <summary>
		/// Changes the Current Value(s) and Available Value(s) of the specified capability to those specified
		/// by the application.
		/// </summary>
		/// Current Values are set when the container is a <see cref="T:NTwain.Data.TWOneValue" /> or <see cref="T:NTwain.Data.TWArray" />. Available and
		/// Current Values are set when the container is a <see cref="T:NTwain.Data.TWEnumeration" /> or <see cref="T:NTwain.Data.TWRange" />.
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode SetConstraint(TWCapability capability)
		{
			Session.VerifyState(4, 7, DataGroups.Control, DataArgumentType.Capability, NTwain.Data.Message.SetConstraint);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataArgumentType.Capability, NTwain.Data.Message.SetConstraint, capability);
		}
	}
	/// <summary>
	/// This is to support custom DAT value for custom capability defined by some manufacturers.
	/// </summary>
	public sealed class CapabilityCustom : TripletBase
	{
		internal CapabilityCustom(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Returns the Source’s Current, Default and Available Values for a specified capability.
		/// </summary>
		/// <param name="customDAT">The custom DAT_* value from manufacturer.</param>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode Get(ushort customDAT, TWCapability capability)
		{
			Session.VerifyState(4, 7, DataGroups.Control, (DataArgumentType)customDAT, NTwain.Data.Message.Get);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, (DataArgumentType)customDAT, NTwain.Data.Message.Get, capability);
		}

		/// <summary>
		/// Returns the Source’s Current Value for the specified capability.
		/// </summary>
		/// <param name="customDAT">The custom DAT_* value from manufacturer.</param>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode GetCurrent(ushort customDAT, TWCapability capability)
		{
			Session.VerifyState(4, 7, DataGroups.Control, (DataArgumentType)customDAT, NTwain.Data.Message.GetCurrent);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, (DataArgumentType)customDAT, NTwain.Data.Message.GetCurrent, capability);
		}

		/// <summary>
		/// Returns the Source’s Default Value. This is the Source’s preferred default value.
		/// </summary>
		/// <param name="customDAT">The custom DAT_* value from manufacturer.</param>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode GetDefault(ushort customDAT, TWCapability capability)
		{
			Session.VerifyState(4, 7, DataGroups.Control, (DataArgumentType)customDAT, NTwain.Data.Message.GetDefault);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, (DataArgumentType)customDAT, NTwain.Data.Message.GetDefault, capability);
		}

		/// <summary>
		/// Returns help text suitable for use in a GUI; for instance: "Specify the amount of detail in an
		/// image. Higher values result in more detail." for ICapXRESOLUTION.
		/// </summary>
		/// <param name="customDAT">The custom DAT_* value from manufacturer.</param>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode GetHelp(ushort customDAT, TWCapability capability)
		{
			Session.VerifyState(4, 4, DataGroups.Control, (DataArgumentType)customDAT, NTwain.Data.Message.GetHelp);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, (DataArgumentType)customDAT, NTwain.Data.Message.GetHelp, capability);
		}

		/// <summary>
		/// Returns a label suitable for use in a GUI, for instance "Resolution:"
		/// for ICapXRESOLUTION.
		/// </summary>
		/// <param name="customDAT">The custom DAT_* value from manufacturer.</param>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode GetLabel(ushort customDAT, TWCapability capability)
		{
			Session.VerifyState(4, 4, DataGroups.Control, (DataArgumentType)customDAT, NTwain.Data.Message.GetLabel);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, (DataArgumentType)customDAT, NTwain.Data.Message.GetLabel, capability);
		}

		/// <summary>
		/// Return all of the labels for a capability of type <see cref="T:NTwain.Data.TWArray" /> or <see cref="T:NTwain.Data.TWEnumeration" />, for example
		/// "US Letter" for ICapSupportedSizes’ TWSS_USLETTER.
		/// </summary>
		/// <param name="customDAT">The custom DAT_* value from manufacturer.</param>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode GetLabelEnum(ushort customDAT, TWCapability capability)
		{
			Session.VerifyState(4, 4, DataGroups.Control, (DataArgumentType)customDAT, NTwain.Data.Message.GetLabelEnum);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, (DataArgumentType)customDAT, NTwain.Data.Message.GetLabelEnum, capability);
		}

		/// <summary>
		/// Returns the Source’s support status of this capability.
		/// </summary>
		/// <param name="customDAT">The custom DAT_* value from manufacturer.</param>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode QuerySupport(ushort customDAT, TWCapability capability)
		{
			Session.VerifyState(4, 7, DataGroups.Control, (DataArgumentType)customDAT, NTwain.Data.Message.QuerySupport);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, (DataArgumentType)customDAT, NTwain.Data.Message.QuerySupport, capability);
		}

		/// <summary>
		/// Change the Current Value of the specified capability back to its power-on value and return the
		/// new Current Value.
		/// </summary>
		/// <param name="customDAT">The custom DAT_* value from manufacturer.</param>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode Reset(ushort customDAT, TWCapability capability)
		{
			Session.VerifyState(4, 4, DataGroups.Control, (DataArgumentType)customDAT, NTwain.Data.Message.Reset);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, (DataArgumentType)customDAT, NTwain.Data.Message.Reset, capability);
		}

		/// <summary>
		/// This command resets all of the current values and constraints to their defaults for all of the
		/// negotiable capabilities supported by the driver.
		/// </summary>
		/// <param name="customDAT">The custom DAT_* value from manufacturer.</param>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode ResetAll(ushort customDAT, TWCapability capability)
		{
			Session.VerifyState(4, 4, DataGroups.Control, (DataArgumentType)customDAT, NTwain.Data.Message.ResetAll);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, (DataArgumentType)customDAT, NTwain.Data.Message.ResetAll, capability);
		}

		/// <summary>
		/// Changes the Current Value(s) and Available Values of the specified capability to those specified
		/// by the application. As of TWAIN 2.2 this only modifies the Current Value of the specified capability, constraints cannot be
		/// changed with this.
		/// Current Values are set when the container is a <see cref="T:NTwain.Data.TWOneValue" /> or <see cref="T:NTwain.Data.TWArray" />. Available and
		/// Current Values are set when the container is a <see cref="T:NTwain.Data.TWEnumeration" /> or <see cref="T:NTwain.Data.TWRange" />.
		/// </summary>
		/// <param name="customDAT">The custom DAT_* value from manufacturer.</param>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode Set(ushort customDAT, TWCapability capability)
		{
			Session.VerifyState(4, 6, DataGroups.Control, (DataArgumentType)customDAT, NTwain.Data.Message.Set);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, (DataArgumentType)customDAT, NTwain.Data.Message.Set, capability);
		}

		/// <summary>
		/// Changes the Current Value(s) and Available Value(s) of the specified capability to those specified
		/// by the application.
		/// </summary>
		/// Current Values are set when the container is a <see cref="T:NTwain.Data.TWOneValue" /> or <see cref="T:NTwain.Data.TWArray" />. Available and
		/// Current Values are set when the container is a <see cref="T:NTwain.Data.TWEnumeration" /> or <see cref="T:NTwain.Data.TWRange" />.
		/// <param name="customDAT">The custom DAT_* value from manufacturer.</param>
		/// <param name="capability">The capability.</param>
		/// <returns></returns>
		public ReturnCode SetConstraint(ushort customDAT, TWCapability capability)
		{
			Session.VerifyState(4, 7, DataGroups.Control, (DataArgumentType)customDAT, NTwain.Data.Message.SetConstraint);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, (DataArgumentType)customDAT, NTwain.Data.Message.SetConstraint, capability);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataGroups.Control" />.
	/// </summary>
	public sealed class DGControl
	{
		private ITwainSessionInternal _session;

		private Callback _callback;

		private Callback2 _callback2;

		private Capability _capability;

		private CapabilityCustom _capabilityCust;

		private CustomDSData _customDSData;

		private DeviceEvent _deviceEvent;

		private EntryPoint _entryPoint;

		private Event _event;

		private FileSystem _fileSys;

		private Identity _identity;

		private Parent _parent;

		private PassThru _passThru;

		private PendingXfers _pendingXfers;

		private SetupFileXfer _setupFileXfer;

		private SetupMemXfer _setupMemXfer;

		private Status _status;

		private StatusUtf8 _statusUtf8;

		private UserInterface _userInterface;

		private XferGroup _xferGroup;

		internal Callback Callback
		{
			get
			{
				if (_callback == null)
				{
					_callback = new Callback(_session);
				}
				return _callback;
			}
		}

		internal Callback2 Callback2
		{
			get
			{
				if (_callback2 == null)
				{
					_callback2 = new Callback2(_session);
				}
				return _callback2;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_CAPABILITY.
		/// </summary>
		public Capability Capability
		{
			get
			{
				if (_capability == null)
				{
					_capability = new Capability(_session);
				}
				return _capability;
			}
		}

		/// <summary>
		/// Gets the operations defined for a custom DAT_* value with capability data.
		/// </summary>
		public CapabilityCustom CapabilityCustom
		{
			get
			{
				if (_capabilityCust == null)
				{
					_capabilityCust = new CapabilityCustom(_session);
				}
				return _capabilityCust;
			}
		}

		internal CustomDSData CustomDSData
		{
			get
			{
				if (_customDSData == null)
				{
					_customDSData = new CustomDSData(_session);
				}
				return _customDSData;
			}
		}

		internal DeviceEvent DeviceEvent
		{
			get
			{
				if (_deviceEvent == null)
				{
					_deviceEvent = new DeviceEvent(_session);
				}
				return _deviceEvent;
			}
		}

		internal EntryPoint EntryPoint
		{
			get
			{
				if (_entryPoint == null)
				{
					_entryPoint = new EntryPoint(_session);
				}
				return _entryPoint;
			}
		}

		internal Event Event
		{
			get
			{
				if (_event == null)
				{
					_event = new Event(_session);
				}
				return _event;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_FILESYSTEM.
		/// </summary>
		public FileSystem FileSystem
		{
			get
			{
				if (_fileSys == null)
				{
					_fileSys = new FileSystem(_session);
				}
				return _fileSys;
			}
		}

		internal Identity Identity
		{
			get
			{
				if (_identity == null)
				{
					_identity = new Identity(_session);
				}
				return _identity;
			}
		}

		internal Parent Parent
		{
			get
			{
				if (_parent == null)
				{
					_parent = new Parent(_session);
				}
				return _parent;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_PASSTHRU.
		/// </summary>
		public PassThru PassThru
		{
			get
			{
				if (_passThru == null)
				{
					_passThru = new PassThru(_session);
				}
				return _passThru;
			}
		}

		internal PendingXfers PendingXfers
		{
			get
			{
				if (_pendingXfers == null)
				{
					_pendingXfers = new PendingXfers(_session);
				}
				return _pendingXfers;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_SETUPFILEXFER.
		/// </summary>
		public SetupFileXfer SetupFileXfer
		{
			get
			{
				if (_setupFileXfer == null)
				{
					_setupFileXfer = new SetupFileXfer(_session);
				}
				return _setupFileXfer;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_SETUPMEMXFER.
		/// </summary>
		public SetupMemXfer SetupMemXfer
		{
			get
			{
				if (_setupMemXfer == null)
				{
					_setupMemXfer = new SetupMemXfer(_session);
				}
				return _setupMemXfer;
			}
		}

		internal Status Status
		{
			get
			{
				if (_status == null)
				{
					_status = new Status(_session);
				}
				return _status;
			}
		}

		internal StatusUtf8 StatusUtf8
		{
			get
			{
				if (_statusUtf8 == null)
				{
					_statusUtf8 = new StatusUtf8(_session);
				}
				return _statusUtf8;
			}
		}

		internal UserInterface UserInterface
		{
			get
			{
				if (_userInterface == null)
				{
					_userInterface = new UserInterface(_session);
				}
				return _userInterface;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_XFERGROUP.
		/// </summary>
		public XferGroup XferGroup
		{
			get
			{
				if (_xferGroup == null)
				{
					_xferGroup = new XferGroup(_session);
				}
				return _xferGroup;
			}
		}

		internal DGControl(ITwainSessionInternal session)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			_session = session;
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.CustomDSData" />.
	/// </summary>
	internal sealed class CustomDSData : TripletBase
	{
		internal CustomDSData(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This operation is used by the application to query the data source for its current settings, e.g.
		/// DPI, paper size, color format. The actual format of the data is data source dependent and not
		/// defined by TWAIN.
		/// </summary>
		/// <param name="customData">The custom data.</param>
		/// <returns></returns>
		public ReturnCode Get(out TWCustomDSData customData)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.CustomDSData, NTwain.Data.Message.Get);
			customData = new TWCustomDSData();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, customData);
		}

		/// <summary>
		/// This operation is used by the application to set the current settings for a data source to a
		/// previous state as defined by the data contained in the customData data structure. The actual
		/// format of the data is data source dependent and not defined by TWAIN.
		/// </summary>
		/// <param name="customData">The custom data.</param>
		/// <returns></returns>
		public ReturnCode Set(TWCustomDSData customData)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.CustomDSData, NTwain.Data.Message.Set);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Set, customData);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.DeviceEvent" />.
	/// </summary>
	internal sealed class DeviceEvent : TripletBase
	{
		internal DeviceEvent(ITwainSessionInternal session)
			: base(session)
		{
		}

		public ReturnCode Get(out TWDeviceEvent sourceDeviceEvent)
		{
			Session.VerifyState(4, 7, DataGroups.Control, DataArgumentType.DeviceEvent, NTwain.Data.Message.Get);
			sourceDeviceEvent = new TWDeviceEvent();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, sourceDeviceEvent);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.EntryPoint" />.
	/// </summary>
	internal sealed class EntryPoint : TripletBase
	{
		internal EntryPoint(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Gets the function entry points for twain 2.0 or higher.
		/// </summary>
		/// <param name="entryPoint">The entry point.</param>
		/// <returns></returns>
		public ReturnCode Get(out TWEntryPoint entryPoint)
		{
			Session.VerifyState(3, 3, DataGroups.Control, DataArgumentType.EntryPoint, NTwain.Data.Message.Get);
			entryPoint = new TWEntryPoint();
			return Dsm.DsmEntry(Session.AppId, NTwain.Data.Message.Get, entryPoint);
		}
	}
	internal sealed class Event : TripletBase
	{
		internal Event(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This operation supports the distribution of events from the application to Sources so that the
		/// Source can maintain its user interface and return messages to the application. Once the
		/// application has enabled the Source, it must immediately begin sending to the Source all events
		/// that enter the application’s main event loop. This allows the Source to update its user interface
		/// in real-time and to return messages to the application which cause state transitions. Even if the
		/// application overrides the Source’s user interface, it must forward all events once the Source has
		/// been enabled. The Source will tell the application whether or not each event belongs to the
		/// Source.
		/// </summary>
		/// <param name="theEvent">The event.</param>
		/// <returns></returns>
		public ReturnCode ProcessEvent(TWEvent theEvent)
		{
			Session.VerifyState(4, 7, DataGroups.Control, DataArgumentType.Event, NTwain.Data.Message.ProcessEvent);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.ProcessEvent, theEvent);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.FileSystem" />.
	/// </summary>
	public sealed class FileSystem : TripletBase
	{
		internal FileSystem(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This operation selects the destination directory within the Source (camera, storage, etc), where
		/// images captured using CapAutomaticCapture will be stored. This command only selects
		/// the destination directory (a file of type Directory). The directory must exist and be
		/// accessible to the Source. The creation of images within the directory is at the discretion of the
		/// Source, and may result in the creation of additional sub-directories.
		/// </summary>
		/// <param name="fileSystem">The file system.</param>
		/// <returns></returns>
		public ReturnCode AutomaticCaptureDirectory(TWFileSystem fileSystem)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.FileSystem, NTwain.Data.Message.AutomaticCaptureDirectory);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.AutomaticCaptureDirectory, fileSystem);
		}

		/// <summary>
		/// This operation selects the current device within the Source (camera, storage, etc). If the device is
		/// a <see cref="F:NTwain.Data.FileType.Domain" />, then this command enters a directory that can contain <see cref="F:NTwain.Data.FileType.Host" /> files. If the
		/// device is a <see cref="F:NTwain.Data.FileType.Host" />, then this command enters a directory that can contain
		/// <see cref="F:NTwain.Data.FileType.Directory" /> files. If the device is a <see cref="F:NTwain.Data.FileType.Directory" />, then this command enters a
		/// directory that can contain <see cref="F:NTwain.Data.FileType.Directory" /> or <see cref="F:NTwain.Data.FileType.Image" /> files.
		/// </summary>
		/// <param name="fileSystem">The file system.</param>
		/// <returns></returns>
		public ReturnCode ChangeDirectory(TWFileSystem fileSystem)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.FileSystem, NTwain.Data.Message.ChangeDirectory);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.ChangeDirectory, fileSystem);
		}

		/// <summary>
		/// This operation copies a file or directory. Absolute and relative pathnames are supported. A file
		/// may not be overwritten with this command. If an Application wishes to do this, it must first
		/// delete the unwanted file and then reissue the Copy command.
		/// The Application specifies the path and name of the entry to be copied in InputName. The
		/// Application specifies the new patch and name in OutputName.
		/// It is not permitted to copy files into the root directory.
		/// </summary>
		/// <param name="fileSystem">The file system.</param>
		/// <returns></returns>
		public ReturnCode Copy(TWFileSystem fileSystem)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.FileSystem, NTwain.Data.Message.Copy);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Copy, fileSystem);
		}

		/// <summary>
		/// This operation creates a new directory within the current directory. Pathnames are not allowed,
		/// only the name of the new directory can be specified.
		/// </summary>
		/// <param name="fileSystem">The file system.</param>
		/// <returns></returns>
		public ReturnCode CreateDirectory(TWFileSystem fileSystem)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.FileSystem, NTwain.Data.Message.CreateDirectory);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.CreateDirectory, fileSystem);
		}

		/// <summary>
		/// This operation deletes a file or directory on the device. Pathnames are not allowed, only the
		/// name of the file or directory to be deleted can be specified. Recursive deletion can be specified
		/// by setting the Recursive to TRUE.
		/// </summary>
		/// <param name="fileSystem">The file system.</param>
		/// <returns></returns>
		public ReturnCode Delete(TWFileSystem fileSystem)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.FileSystem, NTwain.Data.Message.Delete);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Delete, fileSystem);
		}

		/// <summary>
		/// This operation formats the specified storage. This operation destroys all images and subdirectories
		/// under the selected device. Use with caution.
		/// </summary>
		/// <param name="fileSystem">The file system.</param>
		/// <returns></returns>
		public ReturnCode FormatMedia(TWFileSystem fileSystem)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.FileSystem, NTwain.Data.Message.FormatMedia);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.FormatMedia, fileSystem);
		}

		/// <summary>
		/// The operation frees the Context field in fileSystem.
		/// Every call to GetFirstFile must be matched by
		/// a call to GetClose to release the Context field.
		/// </summary>
		/// <param name="fileSystem">The file system.</param>
		/// <returns></returns>
		public ReturnCode GetClose(TWFileSystem fileSystem)
		{
			Session.VerifyState(4, 6, DataGroups.Control, DataArgumentType.FileSystem, NTwain.Data.Message.GetClose);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.GetClose, fileSystem);
		}

		/// <summary>
		/// This operation gets the first filename in a directory, and returns information about that file (the
		/// same information that can be retrieved with GetInfo).
		/// </summary>
		/// <param name="fileSystem">The file system.</param>
		/// <returns></returns>
		public ReturnCode GetFirstFile(TWFileSystem fileSystem)
		{
			Session.VerifyState(4, 6, DataGroups.Control, DataArgumentType.FileSystem, NTwain.Data.Message.GetFirstFile);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.GetFirstFile, fileSystem);
		}

		/// <summary>
		/// This operation fills the information fields in fileSystem.
		/// InputName contains the absolute or relative path and filename of the requested file.
		/// OutputName returns the absolute path to the file.
		/// </summary>
		/// <param name="fileSystem">The file system.</param>
		/// <returns></returns>
		public ReturnCode GetInfo(TWFileSystem fileSystem)
		{
			Session.VerifyState(4, 7, DataGroups.Control, DataArgumentType.FileSystem, NTwain.Data.Message.GetInfo);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.GetInfo, fileSystem);
		}

		/// <summary>
		/// This operation gets the next filename in a directory, and returns information about that file (the
		/// same information that can be retrieved with GetInfo).
		/// </summary>
		/// <param name="fileSystem">The file system.</param>
		/// <returns></returns>
		public ReturnCode GetNextFile(TWFileSystem fileSystem)
		{
			Session.VerifyState(4, 6, DataGroups.Control, DataArgumentType.FileSystem, NTwain.Data.Message.GetNextFile);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.GetNextFile, fileSystem);
		}

		/// <summary>
		/// This operation renames (and optionally moves) a file or directory. Absolute and relative path
		/// names are supported. A file may not be overwritten with this command. If an Application
		/// wishes to do this it must first delete the unwanted file, then issue the rename command.
		/// The Application specifies the path and name of the entry to be renamed in InputName. The
		/// Application specifies the new path and name in OutputName.
		/// Filenames in the root directory cannot be moved or renamed.
		/// </summary>
		/// <param name="fileSystem">The file system.</param>
		/// <returns></returns>
		public ReturnCode Rename(TWFileSystem fileSystem)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.FileSystem, NTwain.Data.Message.Rename);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Rename, fileSystem);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.Identity" />.
	/// </summary>
	internal sealed class Identity : TripletBase
	{
		internal Identity(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// When an application is finished with a Source, it must formally close the session between them
		/// using this operation. This is necessary in case the Source only supports connection with a single
		/// application (many desktop scanners will behave this way). A Source such as this cannot be
		/// accessed by other applications until its current session is terminated.
		/// </summary>
		/// <returns></returns>
		public ReturnCode CloseDS()
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.Identity, NTwain.Data.Message.CloseDS);
			ReturnCode returnCode = Dsm.DsmEntry(Session.AppId, NTwain.Data.Message.CloseDS, Session.CurrentSource.Identity);
			if (returnCode == ReturnCode.Success)
			{
				Session.ChangeCurrentSource(null);
				Session.ChangeState(3, notifyChange: true);
			}
			return returnCode;
		}

		/// <summary>
		/// Gets the identification information of the system default Source.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		public ReturnCode GetDefault(out TWIdentity source)
		{
			Session.VerifyState(3, 7, DataGroups.Control, DataArgumentType.Identity, NTwain.Data.Message.GetDefault);
			source = new TWIdentity();
			return Dsm.DsmEntry(Session.AppId, NTwain.Data.Message.GetDefault, source);
		}

		/// <summary>
		/// The application may obtain the first Source that are currently available on the system which
		/// match the application’s supported groups.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		public ReturnCode GetFirst(out TWIdentity source)
		{
			Session.VerifyState(3, 7, DataGroups.Control, DataArgumentType.Identity, NTwain.Data.Message.GetFirst);
			source = new TWIdentity();
			return Dsm.DsmEntry(Session.AppId, NTwain.Data.Message.GetFirst, source);
		}

		/// <summary>
		/// The application may obtain the next Source that are currently available on the system which
		/// match the application’s supported groups.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		public ReturnCode GetNext(out TWIdentity source)
		{
			Session.VerifyState(3, 7, DataGroups.Control, DataArgumentType.Identity, NTwain.Data.Message.GetNext);
			source = new TWIdentity();
			return Dsm.DsmEntry(Session.AppId, NTwain.Data.Message.GetNext, source);
		}

		/// <summary>
		/// Loads the specified Source into main memory and causes its initialization.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		public ReturnCode OpenDS(DataSource source)
		{
			Session.VerifyState(3, 3, DataGroups.Control, DataArgumentType.Identity, NTwain.Data.Message.OpenDS);
			ReturnCode returnCode = Dsm.DsmEntry(Session.AppId, NTwain.Data.Message.OpenDS, source.Identity);
			if (returnCode == ReturnCode.Success)
			{
				Session.ChangeCurrentSource(source);
				Session.ChangeState(4, notifyChange: true);
			}
			return returnCode;
		}

		/// <summary>
		/// It allows an application to set the
		/// default TWAIN driver, which is reported back by GetDefault.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		public ReturnCode Set(TWIdentity source)
		{
			Session.VerifyState(3, 3, DataGroups.Control, DataArgumentType.Identity, NTwain.Data.Message.Set);
			return Dsm.DsmEntry(Session.AppId, NTwain.Data.Message.Set, source);
		}

		/// <summary>
		/// This operation should be invoked when the user chooses Select Source... from the application’s
		/// File menu (or an equivalent user action). The Source selected becomes the system default Source.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		public ReturnCode UserSelect(out TWIdentity source)
		{
			Session.VerifyState(3, 7, DataGroups.Control, DataArgumentType.Identity, NTwain.Data.Message.UserSelect);
			source = new TWIdentity();
			return Dsm.DsmEntry(Session.AppId, NTwain.Data.Message.UserSelect, source);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.Parent" />.
	/// </summary>
	internal sealed class Parent : TripletBase
	{
		internal Parent(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// When the application has closed all the Sources it had previously opened, and is finished with
		/// the Source Manager (the application plans to initiate no other TWAIN sessions), it must close
		/// the Source Manager.
		/// </summary>
		/// <param name="handle">The handle. On Windows = points to the window handle (hWnd) that will act as the Source’s
		/// "parent". On Macintosh = should be a NULL value.</param>
		/// <returns></returns>
		public ReturnCode CloseDsm(IntPtr handle)
		{
			Session.VerifyState(3, 3, DataGroups.Control, DataArgumentType.Parent, NTwain.Data.Message.CloseDSM);
			ReturnCode returnCode = Dsm.DsmEntry(Session.AppId, null, DataGroups.Control, DataArgumentType.Parent, NTwain.Data.Message.CloseDSM, ref handle);
			if (returnCode == ReturnCode.Success)
			{
				Session.ChangeState(2, notifyChange: true);
			}
			return returnCode;
		}

		/// <summary>
		/// Causes the Source Manager to initialize itself. This operation must be executed before any other
		/// operations will be accepted by the Source Manager.
		/// </summary>
		/// <param name="handle">The handle. On Windows = points to the window handle (hWnd) that will act as the Source’s
		/// "parent". On Macintosh = should be a NULL value.</param>
		/// <returns></returns>
		public ReturnCode OpenDsm(IntPtr handle)
		{
			Session.VerifyState(1, 2, DataGroups.Control, DataArgumentType.Parent, NTwain.Data.Message.OpenDSM);
			ReturnCode returnCode = Dsm.DsmEntry(Session.AppId, null, DataGroups.Control, DataArgumentType.Parent, NTwain.Data.Message.OpenDSM, ref handle);
			if (returnCode == ReturnCode.Success)
			{
				Session.ChangeState(3, notifyChange: true);
			}
			return returnCode;
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.PassThru" />.
	/// </summary>
	public sealed class PassThru : TripletBase
	{
		internal PassThru(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// PASSTHRU is intended for the use of Source writers writing diagnostic applications. It allows
		/// raw communication with the currently selected device in the Source.
		/// </summary>
		/// <param name="sourcePassThru">The source pass thru.</param>
		/// <returns></returns>
		public ReturnCode PassThrough(TWPassThru sourcePassThru)
		{
			Session.VerifyState(4, 7, DataGroups.Control, DataArgumentType.PassThru, NTwain.Data.Message.PassThru);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.PassThru, sourcePassThru);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.PendingXfers" />.
	/// </summary>
	internal sealed class PendingXfers : TripletBase
	{
		internal PendingXfers(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This triplet is used to cancel or terminate a transfer. Issued in state 6, this triplet cancels the next
		/// pending transfer, discards the transfer data, and decrements the pending transfers count. In
		/// state 7, this triplet terminates the current transfer. If any data has not been transferred (this is
		/// only possible during a memory transfer) that data is discarded.
		/// </summary>
		/// <param name="pendingXfers">The pending xfers.</param>
		/// <returns></returns>
		internal ReturnCode EndXfer(TWPendingXfers pendingXfers)
		{
			Session.VerifyState(6, 7, DataGroups.Control, DataArgumentType.PendingXfers, NTwain.Data.Message.EndXfer);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.EndXfer, pendingXfers);
		}

		/// <summary>
		/// Returns the number of transfers the Source is ready to supply to the application, upon demand.
		/// If DAT_XFERGROUP is set to DG_Image, this is the number of images. If DAT_XFERGROUP is set
		/// to DG_AUDIO, this is the number of audio snippets for the current image. If there is no current
		/// image, this call must return Failure / SeqError.
		/// </summary>
		/// <param name="pendingXfers">The pending xfers.</param>
		/// <returns></returns>
		public ReturnCode Get(TWPendingXfers pendingXfers)
		{
			Session.VerifyState(4, 7, DataGroups.Control, DataArgumentType.PendingXfers, NTwain.Data.Message.Get);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, pendingXfers);
		}

		/// <summary>
		/// Sets the number of pending transfers in the Source to zero.
		/// </summary>
		/// <param name="pendingXfers">The pending xfers.</param>
		/// <returns></returns>
		internal ReturnCode Reset(TWPendingXfers pendingXfers)
		{
			Session.VerifyState(6, 6, DataGroups.Control, DataArgumentType.PendingXfers, NTwain.Data.Message.Reset);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Reset, pendingXfers);
		}

		/// <summary>
		/// If CapAutoScan is TRUE, this command will stop the operation of the scanner’s automatic
		/// feeder. No other action is taken.
		/// </summary>
		/// <param name="pendingXfers">The pending xfers.</param>
		/// <returns></returns>
		public ReturnCode StopFeeder(TWPendingXfers pendingXfers)
		{
			Session.VerifyState(6, 6, DataGroups.Control, DataArgumentType.PendingXfers, NTwain.Data.Message.StopFeeder);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.StopFeeder, pendingXfers);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.SetupFileXfer" />.
	/// </summary>
	public sealed class SetupFileXfer : TripletBase
	{
		internal SetupFileXfer(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Returns information about the file into which the Source has or will put the acquired image
		/// or audio data.
		/// </summary>
		/// <param name="setupFileXfer">The setup file xfer.</param>
		/// <returns></returns>
		public ReturnCode Get(out TWSetupFileXfer setupFileXfer)
		{
			Session.VerifyState(4, 6, DataGroups.Control, DataArgumentType.SetupFileXfer, NTwain.Data.Message.Get);
			setupFileXfer = new TWSetupFileXfer();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, setupFileXfer);
		}

		/// <summary>
		/// Returns information for the default image or audio file.
		/// </summary>
		/// <param name="setupFileXfer">The setup file xfer.</param>
		/// <returns></returns>
		public ReturnCode GetDefault(out TWSetupFileXfer setupFileXfer)
		{
			Session.VerifyState(4, 6, DataGroups.Control, DataArgumentType.SetupFileXfer, NTwain.Data.Message.GetDefault);
			setupFileXfer = new TWSetupFileXfer();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.GetDefault, setupFileXfer);
		}

		/// <summary>
		/// Resets the current file information to the image or audio default file information and
		/// returns that default information.
		/// </summary>
		/// <param name="setupFileXfer">The setup file xfer.</param>
		/// <returns></returns>
		public ReturnCode Reset(out TWSetupFileXfer setupFileXfer)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.SetupFileXfer, NTwain.Data.Message.Reset);
			setupFileXfer = new TWSetupFileXfer();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Reset, setupFileXfer);
		}

		/// <summary>
		/// Sets the file transfer information for the next file transfer. The application is responsible for
		/// verifying that the specified file name is valid and that the file either does not currently exist (in
		/// which case, the Source is to create the file), or that the existing file is available for opening and
		/// read/write operations. The application should also assure that the file format it is requesting
		/// can be provided by the Source
		/// </summary>
		/// <param name="setupFileXfer">The setup file xfer.</param>
		/// <returns></returns>
		public ReturnCode Set(TWSetupFileXfer setupFileXfer)
		{
			Session.VerifyState(4, 6, DataGroups.Control, DataArgumentType.SetupFileXfer, NTwain.Data.Message.Set);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Set, setupFileXfer);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.SetupMemXfer" />.
	/// </summary>
	public sealed class SetupMemXfer : TripletBase
	{
		internal SetupMemXfer(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Returns the Source’s preferred, minimum, and maximum allocation sizes for transfer memory
		/// buffers.
		/// </summary>
		/// <param name="setupMemXfer">The setup mem xfer.</param>
		/// <returns></returns>
		public ReturnCode Get(out TWSetupMemXfer setupMemXfer)
		{
			Session.VerifyState(4, 6, DataGroups.Control, DataArgumentType.SetupMemXfer, NTwain.Data.Message.Get);
			setupMemXfer = new TWSetupMemXfer();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, setupMemXfer);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.Status" />.
	/// </summary>
	internal sealed class Status : TripletBase
	{
		internal Status(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Returns the current Condition Code for the Source Manager.
		/// </summary>
		/// <param name="status">The status.</param>
		/// <returns></returns>
		public ReturnCode GetManager(out TWStatus status)
		{
			Session.VerifyState(2, 7, DataGroups.Control, DataArgumentType.Status, NTwain.Data.Message.Get);
			status = new TWStatus();
			return Dsm.DsmEntry(Session.AppId, null, NTwain.Data.Message.Get, status);
		}

		/// <summary>
		/// Returns the current Condition Code for the specified Source.
		/// </summary>
		/// <param name="status">The status.</param>
		/// <returns></returns>
		public ReturnCode GetSource(out TWStatus status)
		{
			Session.VerifyState(4, 7, DataGroups.Control, DataArgumentType.Status, NTwain.Data.Message.Get);
			status = new TWStatus();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, status);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.StatusUtf8" />.
	/// </summary>
	internal sealed class StatusUtf8 : TripletBase
	{
		internal StatusUtf8(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Translate the contents of a TW_STATUS structure received from the manager into a localized UTF-8
		/// encoded string.
		/// </summary>
		/// <param name="status">The status.</param>
		/// <returns></returns>
		public ReturnCode GetManager(TWStatus status, out TWStatusUtf8 utf8status)
		{
			utf8status = new TWStatusUtf8
			{
				Status = status
			};
			Session.VerifyState(3, 7, DataGroups.Control, DataArgumentType.StatusUtf8, NTwain.Data.Message.Get);
			return Dsm.DsmEntry(Session.AppId, null, NTwain.Data.Message.Get, status);
		}

		/// <summary>
		/// Translate the contents of a TW_STATUS structure received from a Source into a localized UTF-8
		/// encoded string.
		/// </summary>
		/// <param name="status">The status.</param>
		/// <returns></returns>
		public ReturnCode GetSource(TWStatus status, out TWStatusUtf8 utf8status)
		{
			utf8status = new TWStatusUtf8
			{
				Status = status
			};
			Session.VerifyState(3, 7, DataGroups.Control, DataArgumentType.StatusUtf8, NTwain.Data.Message.Get);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, status);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.UserInterface" />.
	/// </summary>
	internal sealed class UserInterface : TripletBase
	{
		internal UserInterface(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This operation causes the Source’s user interface, if displayed during the
		/// EnableDS operation, to be lowered. The Source is returned to
		/// State 4, where capability negotiation can again occur. The application can invoke this operation
		/// either because it wants to shut down the current session, or in response to the Source "posting"
		/// a CloseDSReq event to it. Rarely, the application may need to close the Source because an
		/// error condition was detected.
		/// </summary>
		/// <param name="userInterface">The user interface.</param>
		/// <returns></returns>
		public ReturnCode DisableDS(TWUserInterface userInterface)
		{
			Session.VerifyState(5, 5, DataGroups.Control, DataArgumentType.UserInterface, NTwain.Data.Message.DisableDS);
			ReturnCode returnCode = Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.DisableDS, userInterface);
			if (returnCode == ReturnCode.Success)
			{
				Session.ChangeState(4, notifyChange: true);
			}
			return returnCode;
		}

		/// <summary>
		/// Enables the DS.
		/// </summary>
		/// <param name="userInterface">The user interface.</param>
		/// <returns></returns>
		public ReturnCode EnableDS(TWUserInterface userInterface)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.UserInterface, NTwain.Data.Message.EnableDS);
			using ICommittable committable = Session.GetPendingStateChanger(5);
			ReturnCode returnCode = Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.EnableDS, userInterface);
			if (returnCode == ReturnCode.Success || (!userInterface.ShowUI && returnCode == ReturnCode.CheckStatus))
			{
				committable.Commit();
			}
			return returnCode;
		}

		/// <summary>
		/// This operation is used by applications
		/// that wish to display the source user interface to allow the user to manipulate the sources current
		/// settings for DPI, paper size, etc. but not acquire an image.
		/// </summary>
		/// <param name="userInterface">The user interface.</param>
		/// <returns></returns>
		public ReturnCode EnableDSUIOnly(TWUserInterface userInterface)
		{
			Session.VerifyState(4, 4, DataGroups.Control, DataArgumentType.UserInterface, NTwain.Data.Message.EnableDSUIOnly);
			using ICommittable committable = Session.GetPendingStateChanger(5);
			ReturnCode returnCode = Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.EnableDSUIOnly, userInterface);
			if (returnCode == ReturnCode.Success)
			{
				committable.Commit();
			}
			return returnCode;
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.XferGroup" />.
	/// </summary>
	public sealed class XferGroup : TripletBase
	{
		internal XferGroup(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Returns the Data Group (the type of data) for the upcoming transfer. The Source is required to
		/// only supply one of the DGs specified in the SupportedGroups field of origin.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public ReturnCode Get(ref DataGroups value)
		{
			Session.VerifyState(4, 6, DataGroups.Control, DataArgumentType.XferGroup, NTwain.Data.Message.Get);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataGroups.Control, DataArgumentType.XferGroup, NTwain.Data.Message.Get, ref value);
		}

		/// <summary>
		/// The transfer group determines the kind of data being passed from the Source to the Application.
		/// By default a TWAIN Source must default to DG_IMAGE. Currently the only other data group
		/// supported is DG_AUDIO, which is a feature supported by some digital cameras.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public ReturnCode Set(DataGroups value)
		{
			Session.VerifyState(6, 6, DataGroups.Control, DataArgumentType.XferGroup, NTwain.Data.Message.Set);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataGroups.Control, DataArgumentType.XferGroup, NTwain.Data.Message.Set, ref value);
		}
	}
	/// <summary>
	/// Provides direct access to the triplet call.
	/// </summary>
	public class DGCustom
	{
		private ITwainSessionInternal _session;

		internal DGCustom(ITwainSessionInternal session)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			_session = session;
		}

		/// <summary>
		/// Direct DSM_Entry call with full arguments for custom values.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <param name="dat">The dat.</param>
		/// <param name="message">The message.</param>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		public ReturnCode DsmEntry(DataGroups group, DataArgumentType dat, NTwain.Data.Message message, ref IntPtr data)
		{
			_session.VerifyState(3, 7, group, dat, message);
			return Dsm.DsmEntry(_session.AppId, _session.CurrentSource.Identity, group, dat, message, ref data);
		}

		/// <summary>
		/// Direct DSM_Entry call with full arguments for custom values.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <param name="dat">The dat.</param>
		/// <param name="message">The message.</param>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		public ReturnCode DsmEntry(DataGroups group, DataArgumentType dat, NTwain.Data.Message message, IntPtr data)
		{
			_session.VerifyState(3, 7, group, dat, message);
			return Dsm.DsmEntry(_session.AppId, _session.CurrentSource.Identity, group, dat, message, data);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.CieColor" />.
	/// </summary>
	public sealed class CieColor : TripletBase
	{
		internal CieColor(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This operation causes the Source to report the currently active parameters to be used in
		/// converting acquired color data into CIE XYZ.
		/// </summary>
		/// <param name="cieColor">Color data.</param>
		/// <returns></returns>
		public ReturnCode Get(out TWCieColor cieColor)
		{
			Session.VerifyState(4, 6, DataGroups.Image, DataArgumentType.CieColor, NTwain.Data.Message.Get);
			cieColor = new TWCieColor();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, cieColor);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataGroups.Image" />.
	/// </summary>
	public sealed class DGImage
	{
		private ITwainSessionInternal _session;

		private CieColor _cieColor;

		private ExtImageInfo _extImgInfo;

		private Filter _filter;

		private GrayResponse _grayResponse;

		private IccProfile _iccProfile;

		private ImageFileXfer _imgFileXfer;

		private ImageInfo _imgInfo;

		private ImageLayout _imgLayout;

		private ImageMemFileXfer _imgMemFileXfer;

		private ImageMemXfer _imgMemXfer;

		private ImageNativeXfer _imgNativeXfer;

		private JpegCompression _jpegComp;

		private Palette8 _palette8;

		private RgbResponse _rgbResp;

		/// <summary>
		/// Gets the operations defined for DAT_CIECOLOR.
		/// </summary>
		public CieColor CieColor
		{
			get
			{
				if (_cieColor == null)
				{
					_cieColor = new CieColor(_session);
				}
				return _cieColor;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_EXTIMAGEINFO.
		/// </summary>
		internal ExtImageInfo ExtImageInfo
		{
			get
			{
				if (_extImgInfo == null)
				{
					_extImgInfo = new ExtImageInfo(_session);
				}
				return _extImgInfo;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_FILTER.
		/// </summary>
		public Filter Filter
		{
			get
			{
				if (_filter == null)
				{
					_filter = new Filter(_session);
				}
				return _filter;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_GRAYRESPONSE.
		/// </summary>
		public GrayResponse GrayResponse
		{
			get
			{
				if (_grayResponse == null)
				{
					_grayResponse = new GrayResponse(_session);
				}
				return _grayResponse;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_ICCPROFILE.
		/// </summary>
		public IccProfile IccProfile
		{
			get
			{
				if (_iccProfile == null)
				{
					_iccProfile = new IccProfile(_session);
				}
				return _iccProfile;
			}
		}

		internal ImageFileXfer ImageFileXfer
		{
			get
			{
				if (_imgFileXfer == null)
				{
					_imgFileXfer = new ImageFileXfer(_session);
				}
				return _imgFileXfer;
			}
		}

		internal ImageInfo ImageInfo
		{
			get
			{
				if (_imgInfo == null)
				{
					_imgInfo = new ImageInfo(_session);
				}
				return _imgInfo;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_IMAGELAYOUT.
		/// </summary>
		public ImageLayout ImageLayout
		{
			get
			{
				if (_imgLayout == null)
				{
					_imgLayout = new ImageLayout(_session);
				}
				return _imgLayout;
			}
		}

		internal ImageMemFileXfer ImageMemFileXfer
		{
			get
			{
				if (_imgMemFileXfer == null)
				{
					_imgMemFileXfer = new ImageMemFileXfer(_session);
				}
				return _imgMemFileXfer;
			}
		}

		internal ImageMemXfer ImageMemXfer
		{
			get
			{
				if (_imgMemXfer == null)
				{
					_imgMemXfer = new ImageMemXfer(_session);
				}
				return _imgMemXfer;
			}
		}

		internal ImageNativeXfer ImageNativeXfer
		{
			get
			{
				if (_imgNativeXfer == null)
				{
					_imgNativeXfer = new ImageNativeXfer(_session);
				}
				return _imgNativeXfer;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_JPEGCOMPRESSION.
		/// </summary>
		public JpegCompression JpegCompression
		{
			get
			{
				if (_jpegComp == null)
				{
					_jpegComp = new JpegCompression(_session);
				}
				return _jpegComp;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_PALETTE8.
		/// </summary>
		public Palette8 Palette8
		{
			get
			{
				if (_palette8 == null)
				{
					_palette8 = new Palette8(_session);
				}
				return _palette8;
			}
		}

		/// <summary>
		/// Gets the operations defined for DAT_RGBRESPONSE.
		/// </summary>
		public RgbResponse RgbResponse
		{
			get
			{
				if (_rgbResp == null)
				{
					_rgbResp = new RgbResponse(_session);
				}
				return _rgbResp;
			}
		}

		internal DGImage(ITwainSessionInternal session)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			_session = session;
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.ExtImageInfo" />.
	/// </summary>
	internal sealed class ExtImageInfo : TripletBase
	{
		internal ExtImageInfo(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This operation is used by the application to query the data source for extended image attributes.
		/// The application is responsible for creating and disiposing the info object.
		/// </summary>
		/// <param name="info">The information.</param>
		/// <returns></returns>
		public ReturnCode Get(TWExtImageInfo info)
		{
			Session.VerifyState(7, 7, DataGroups.Image, DataArgumentType.ExtImageInfo, NTwain.Data.Message.Get);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, info);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.Filter" />.
	/// </summary>
	public sealed class Filter : TripletBase
	{
		internal Filter(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Causes the Source to return the filter parameters that will be used during the next image
		/// acquisition.
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <returns></returns>
		public ReturnCode Get(out TWFilter filter)
		{
			Session.VerifyState(4, 6, DataGroups.Image, DataArgumentType.Filter, NTwain.Data.Message.Get);
			filter = new TWFilter();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, filter);
		}

		/// <summary>
		/// Causes the Source to return the power-on default values applied to the filter.
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <returns></returns>
		public ReturnCode GetDefault(out TWFilter filter)
		{
			Session.VerifyState(4, 6, DataGroups.Image, DataArgumentType.Filter, NTwain.Data.Message.GetDefault);
			filter = new TWFilter();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.GetDefault, filter);
		}

		/// <summary>
		/// Allows the Application to configure the filter parameters that will be used during the next image
		/// acquisition.
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <returns></returns>
		public ReturnCode Set(TWFilter filter)
		{
			Session.VerifyState(4, 4, DataGroups.Image, DataArgumentType.Filter, NTwain.Data.Message.Set);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Set, filter);
		}

		/// <summary>
		/// Return the Source to using its power-on default values when it is applying the filter.
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <returns></returns>
		public ReturnCode Reset(out TWFilter filter)
		{
			Session.VerifyState(4, 4, DataGroups.Image, DataArgumentType.Filter, NTwain.Data.Message.Reset);
			filter = new TWFilter();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Reset, filter);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.GrayResponse" />.
	/// </summary>
	public sealed class GrayResponse : TripletBase
	{
		internal GrayResponse(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// The Reset operation causes the Source to use its "identity response curve." The identity
		/// curve causes no change in the values of the captured data when it is applied.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <returns></returns>
		public ReturnCode Reset(out TWGrayResponse response)
		{
			Session.VerifyState(4, 4, DataGroups.Image, DataArgumentType.GrayResponse, NTwain.Data.Message.Reset);
			response = new TWGrayResponse();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Reset, response);
		}

		/// <summary>
		/// This operation causes the Source to transform any grayscale data according to the response
		/// curve specified.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <returns></returns>
		public ReturnCode Set(TWGrayResponse response)
		{
			Session.VerifyState(4, 4, DataGroups.Image, DataArgumentType.GrayResponse, NTwain.Data.Message.Set);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Set, response);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.IccProfile" />.
	/// </summary>
	public sealed class IccProfile : TripletBase
	{
		internal IccProfile(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This operation provides the application with the ICC profile associated with the image which is
		/// about to be transferred (state 6) or is being transferred (state 7).
		/// </summary>
		/// <param name="profile">The profile.</param>
		/// <returns></returns>
		public ReturnCode Get(ref TWMemory profile)
		{
			Session.VerifyState(6, 7, DataGroups.Image, DataArgumentType.IccProfile, NTwain.Data.Message.Get);
			profile = default;
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataArgumentType.IccProfile, NTwain.Data.Message.Get, ref profile);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.ImageFileXfer" />.
	/// </summary>
	internal sealed class ImageFileXfer : TripletBase
	{
		internal ImageFileXfer(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This operation is used to initiate the transfer of an image from the Source to the application via
		/// the disk-file transfer mechanism. It causes the transfer to begin.
		/// </summary>
		/// <returns></returns>
		public ReturnCode Get()
		{
			Session.VerifyState(6, 6, DataGroups.Image, DataArgumentType.ImageFileXfer, NTwain.Data.Message.Get);
			IntPtr data = IntPtr.Zero;
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataGroups.Image, DataArgumentType.ImageFileXfer, NTwain.Data.Message.Get, ref data);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.ImageInfo" />.
	/// </summary>
	internal sealed class ImageInfo : TripletBase
	{
		internal ImageInfo(ITwainSessionInternal session)
			: base(session)
		{
		}

		public ReturnCode Get(out TWImageInfo info)
		{
			Session.VerifyState(6, 7, DataGroups.Image, DataArgumentType.ImageInfo, NTwain.Data.Message.Get);
			info = new TWImageInfo();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, info);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.ImageLayout" />.
	/// </summary>
	public sealed class ImageLayout : TripletBase
	{
		internal ImageLayout(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Gets both the size and placement of the image on the scanner. The
		/// coordinates on the scanner and the extents of the image are expressed in the unit of measure
		/// currently negotiated for ICAP_UNITS (default is inches).
		/// </summary>
		/// <param name="layout">The layout.</param>
		/// <returns></returns>
		public ReturnCode Get(out TWImageLayout layout)
		{
			Session.VerifyState(4, 6, DataGroups.Image, DataArgumentType.ImageLayout, NTwain.Data.Message.Get);
			layout = new TWImageLayout();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, layout);
		}

		/// <summary>
		/// returns the default information on the layout of an image. This is the size and
		/// position of the image that will be acquired from the Source if the acquisition is started with the
		/// Source (and the device it is controlling) in its power-on state (for instance, most flatbed scanners
		/// will capture the entire bed).
		/// </summary>
		/// <param name="layout">The layout.</param>
		/// <returns></returns>
		public ReturnCode GetDefault(out TWImageLayout layout)
		{
			Session.VerifyState(4, 6, DataGroups.Image, DataArgumentType.ImageLayout, NTwain.Data.Message.GetDefault);
			layout = new TWImageLayout();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.GetDefault, layout);
		}

		/// <summary>
		/// This operation sets the image layout information for the next transfer to its default settings.
		/// </summary>
		/// <param name="layout">The layout.</param>
		/// <returns></returns>
		public ReturnCode Reset(out TWImageLayout layout)
		{
			Session.VerifyState(4, 4, DataGroups.Image, DataArgumentType.ImageLayout, NTwain.Data.Message.Reset);
			layout = new TWImageLayout();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Reset, layout);
		}

		/// <summary>
		/// This operation sets the layout for the next image transfer. This allows the application to specify
		/// the physical area to be acquired during the next image transfer (for instance, a frame-based
		/// application would pass to the Source the size of the frame the user selected within the
		/// application—the helpful Source would present a selection region already sized to match the
		/// layout frame size).
		/// </summary>
		/// <param name="layout">The layout.</param>
		/// <returns></returns>
		public ReturnCode Set(TWImageLayout layout)
		{
			Session.VerifyState(4, 4, DataGroups.Image, DataArgumentType.ImageLayout, NTwain.Data.Message.Set);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Set, layout);
		}
	}
	internal sealed class ImageMemFileXfer : TripletBase
	{
		internal ImageMemFileXfer(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This operation is used to initiate the transfer of an image from the Source to the application via
		/// the Memory-File transfer mechanism.
		/// </summary>
		/// <param name="xfer">The xfer.</param>
		/// <returns></returns>
		public ReturnCode Get(TWImageMemXfer xfer)
		{
			Session.VerifyState(6, 7, DataGroups.Image, DataArgumentType.ImageMemFileXfer, NTwain.Data.Message.Get);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, xfer);
		}
	}
	internal sealed class ImageMemXfer : TripletBase
	{
		internal ImageMemXfer(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This operation is used to initiate the transfer of an image from the Source to the application via
		/// the Buffered Memory transfer mechanism.
		/// </summary>
		/// <param name="xfer">The xfer.</param>
		/// <returns></returns>
		public ReturnCode Get(TWImageMemXfer xfer)
		{
			Session.VerifyState(6, 7, DataGroups.Image, DataArgumentType.ImageMemXfer, NTwain.Data.Message.Get);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, xfer);
		}
	}
	internal sealed class ImageNativeXfer : TripletBase
	{
		internal ImageNativeXfer(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Causes the transfer of an image’s data from the Source to the application, via the Native transfer
		/// mechanism, to begin. The resulting data is stored in main memory in a single block. The data is
		/// stored in Picture (PICT) format on the Macintosh and as a device-independent bitmap (DIB)
		/// under Microsoft Windows. The size of the image that can be transferred is limited to the size of
		/// the memory block that can be allocated by the Source.
		/// </summary>
		/// <param name="handle">The handle.</param>
		/// <returns></returns>
		public ReturnCode Get(ref IntPtr handle)
		{
			Session.VerifyState(6, 6, DataGroups.Image, DataArgumentType.ImageNativeXfer, NTwain.Data.Message.Get);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataGroups.Image, DataArgumentType.ImageNativeXfer, NTwain.Data.Message.Get, ref handle);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.JpegCompression" />.
	/// </summary>
	public sealed class JpegCompression : TripletBase
	{
		internal JpegCompression(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Causes the Source to return the parameters that will be used during the compression of data
		/// using the JPEG algorithms.
		/// </summary>
		/// <param name="compression">The compression.</param>
		/// <returns></returns>
		public ReturnCode Get(out TWJpegCompression compression)
		{
			Session.VerifyState(4, 6, DataGroups.Image, DataArgumentType.JpegCompression, NTwain.Data.Message.Get);
			compression = new TWJpegCompression();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, compression);
		}

		/// <summary>
		/// Causes the Source to return the power-on default values applied to JPEG-compressed data
		/// transfers.
		/// </summary>
		/// <param name="compression">The compression.</param>
		/// <returns></returns>
		public ReturnCode GetDefault(out TWJpegCompression compression)
		{
			Session.VerifyState(4, 6, DataGroups.Image, DataArgumentType.JpegCompression, NTwain.Data.Message.GetDefault);
			compression = new TWJpegCompression();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.GetDefault, compression);
		}

		/// <summary>
		/// Return the Source to using its power-on default values for JPEG-compressed transfers.
		/// </summary>
		/// <param name="compression">The compression.</param>
		/// <returns></returns>
		public ReturnCode Reset(out TWJpegCompression compression)
		{
			Session.VerifyState(4, 4, DataGroups.Image, DataArgumentType.JpegCompression, NTwain.Data.Message.Reset);
			compression = new TWJpegCompression();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Reset, compression);
		}

		/// <summary>
		/// Allows the application to configure the compression parameters to be used on all future JPEGcompressed
		/// transfers during the current session. The application should have already
		/// established that the requested values are supported by the Source.
		/// </summary>
		/// <param name="compression">The compression.</param>
		/// <returns></returns>
		public ReturnCode Set(TWJpegCompression compression)
		{
			Session.VerifyState(4, 4, DataGroups.Image, DataArgumentType.JpegCompression, NTwain.Data.Message.Set);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Set, compression);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.Palette8" />.
	/// </summary>
	public sealed class Palette8 : TripletBase
	{
		internal Palette8(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// This operation causes the Source to report its current palette information.
		/// </summary>
		/// <param name="palette">The palette.</param>
		/// <returns></returns>
		public ReturnCode Get(out TWPalette8 palette)
		{
			Session.VerifyState(4, 6, DataGroups.Image, DataArgumentType.Palette8, NTwain.Data.Message.Get);
			palette = new TWPalette8();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Get, palette);
		}

		/// <summary>
		/// This operation causes the Source to report its power-on default palette.
		/// </summary>
		/// <param name="palette">The palette.</param>
		/// <returns></returns>
		public ReturnCode GetDefault(out TWPalette8 palette)
		{
			Session.VerifyState(4, 6, DataGroups.Image, DataArgumentType.Palette8, NTwain.Data.Message.GetDefault);
			palette = new TWPalette8();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.GetDefault, palette);
		}

		/// <summary>
		/// This operation causes the Source to dispose of any current palette it has and to use its default
		/// palette for the next palette transfer.
		/// </summary>
		/// <param name="palette">The palette.</param>
		/// <returns></returns>
		public ReturnCode Reset(out TWPalette8 palette)
		{
			Session.VerifyState(4, 4, DataGroups.Image, DataArgumentType.Palette8, NTwain.Data.Message.Reset);
			palette = new TWPalette8();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Reset, palette);
		}

		/// <summary>
		/// This operation requests that the Source adopt the specified palette for use with all subsequent
		/// palette transfers. The application should be careful to supply a palette that matches the bit
		/// depth of the Source. The Source is not required to adopt this palette. The application should be
		/// careful to check the return value from this operation.
		/// </summary>
		/// <param name="palette">The palette.</param>
		/// <returns></returns>
		public ReturnCode Set(TWPalette8 palette)
		{
			Session.VerifyState(4, 4, DataGroups.Image, DataArgumentType.Palette8, NTwain.Data.Message.Set);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Set, palette);
		}
	}
	/// <summary>
	/// Represents <see cref="F:NTwain.Data.DataArgumentType.RgbResponse" />.
	/// </summary>
	public sealed class RgbResponse : TripletBase
	{
		internal RgbResponse(ITwainSessionInternal session)
			: base(session)
		{
		}

		/// <summary>
		/// Causes the Source to use its "identity" response curves for future RGB transfers. The identity
		/// curve causes no change in the values of the captured data when it is applied. (Note that
		/// resetting the curves for RGB data does not reset any curves for other pixel types).
		/// </summary>
		/// <param name="response">The response.</param>
		/// <returns></returns>
		public ReturnCode Reset(out TWRgbResponse response)
		{
			Session.VerifyState(4, 4, DataGroups.Image, DataArgumentType.RgbResponse, NTwain.Data.Message.Reset);
			response = new TWRgbResponse();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Reset, response);
		}

		/// <summary>
		/// Causes the Source to transform any RGB data according to the response curves specified by the
		/// application.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <returns></returns>
		public ReturnCode Set(TWRgbResponse response)
		{
			Session.VerifyState(4, 4, DataGroups.Image, DataArgumentType.RgbResponse, NTwain.Data.Message.Set);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, NTwain.Data.Message.Set, response);
		}
	}
	/// <summary>
	/// Contains the full TWAIN entry signatures through pinvoke.
	/// These are called by all the defined triplets methods in 
	/// this namespace.
	/// </summary>
	internal static class Dsm
	{
		private static class NativeMethods
		{
			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, ref IntPtr data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, IntPtr data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, ref DataGroups data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWAudioInfo data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCapability data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCustomDSData data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWDeviceEvent data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCallback data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCallback2 data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWEntryPoint data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWEvent data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWFileSystem data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, IntPtr zero, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWIdentity data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWPassThru data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWPendingXfers data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWSetupFileXfer data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWSetupMemXfer data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWStatusUtf8 data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWUserInterface data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCieColor data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWExtImageInfo data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWFilter data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWGrayResponse data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWImageInfo data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWImageLayout data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWImageMemXfer data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWJpegCompression data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWPalette8 data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWRgbResponse data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWStatus data);

			[DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmLinux([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, ref TWMemory data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, ref IntPtr data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, IntPtr data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, ref DataGroups data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWAudioInfo data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCapability data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCustomDSData data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWDeviceEvent data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCallback data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCallback2 data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWEntryPoint data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWEvent data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWFileSystem data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, IntPtr zero, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWIdentity data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWPassThru data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWPendingXfers data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWSetupFileXfer data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWSetupMemXfer data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWStatusUtf8 data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWUserInterface data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCieColor data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWExtImageInfo data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWFilter data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWGrayResponse data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWImageInfo data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWImageLayout data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWImageMemXfer data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWJpegCompression data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWPalette8 data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWRgbResponse data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWStatus data);

			[DllImport("twaindsm.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinNew([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, ref TWMemory data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, ref IntPtr data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, IntPtr data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, ref DataGroups data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWAudioInfo data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCapability data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCustomDSData data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWDeviceEvent data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCallback data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCallback2 data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWEntryPoint data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWEvent data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWFileSystem data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, IntPtr zero, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWIdentity data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWPassThru data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWPendingXfers data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWSetupFileXfer data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWSetupMemXfer data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWStatusUtf8 data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWUserInterface data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWCieColor data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWExtImageInfo data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWFilter data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWGrayResponse data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWImageInfo data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWImageLayout data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWImageMemXfer data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWJpegCompression data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWPalette8 data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWRgbResponse data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, [In][Out] TWStatus data);

			[DllImport("twain_32.dll", EntryPoint = "DSM_Entry")]
			public static extern ReturnCode DsmWinOld([In][Out] TWIdentity origin, [In][Out] TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, ref TWMemory data);
		}

		internal const string DSM_ENTRY = "DSM_Entry";

		internal const string LINUX_DSM_PATH = "/usr/local/lib/libtwaindsm.so";

		internal const string WIN_NEW_DSM_NAME = "twaindsm.dll";

		internal const string WIN_OLD_DSM_NAME = "twain_32.dll";

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, ref IntPtr data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, dg, dat, msg, ref data);
				}
				return NativeMethods.DsmWinOld(origin, destination, dg, dat, msg, ref data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, dg, dat, msg, ref data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, IntPtr data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, dg, dat, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, dg, dat, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, dg, dat, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, DataGroups dg, DataArgumentType dat, NTwain.Data.Message msg, ref DataGroups data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, dg, dat, msg, ref data);
				}
				return NativeMethods.DsmWinOld(origin, destination, dg, dat, msg, ref data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, dg, dat, msg, ref data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWAudioInfo data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Audio, DataArgumentType.AudioInfo, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Audio, DataArgumentType.AudioInfo, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Audio, DataArgumentType.AudioInfo, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, DataArgumentType dat, NTwain.Data.Message msg, TWCapability data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, dat, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, dat, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, dat, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWCustomDSData data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, DataArgumentType.CustomDSData, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, DataArgumentType.CustomDSData, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, DataArgumentType.CustomDSData, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWDeviceEvent data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, DataArgumentType.DeviceEvent, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, DataArgumentType.DeviceEvent, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, DataArgumentType.DeviceEvent, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWCallback data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, DataArgumentType.Callback, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, DataArgumentType.Callback, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, DataArgumentType.Callback, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWCallback2 data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, DataArgumentType.Callback2, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, DataArgumentType.Callback2, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, DataArgumentType.Callback2, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, NTwain.Data.Message msg, TWEntryPoint data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, null, DataGroups.Control, DataArgumentType.EntryPoint, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, null, DataGroups.Control, DataArgumentType.EntryPoint, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, null, DataGroups.Control, DataArgumentType.EntryPoint, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWEvent data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, DataArgumentType.Event, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, DataArgumentType.Event, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, DataArgumentType.Event, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWFileSystem data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, DataArgumentType.FileSystem, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, DataArgumentType.FileSystem, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, DataArgumentType.FileSystem, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, NTwain.Data.Message msg, TWIdentity data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, IntPtr.Zero, DataGroups.Control, DataArgumentType.Identity, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, IntPtr.Zero, DataGroups.Control, DataArgumentType.Identity, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, IntPtr.Zero, DataGroups.Control, DataArgumentType.Identity, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWPassThru data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, DataArgumentType.PassThru, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, DataArgumentType.PassThru, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, DataArgumentType.PassThru, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWPendingXfers data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, DataArgumentType.PendingXfers, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, DataArgumentType.PendingXfers, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, DataArgumentType.PendingXfers, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWSetupFileXfer data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, DataArgumentType.SetupFileXfer, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, DataArgumentType.SetupFileXfer, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, DataArgumentType.SetupFileXfer, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWSetupMemXfer data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, DataArgumentType.SetupMemXfer, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, DataArgumentType.SetupMemXfer, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, DataArgumentType.SetupMemXfer, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWStatusUtf8 data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, DataArgumentType.StatusUtf8, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, DataArgumentType.StatusUtf8, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, DataArgumentType.StatusUtf8, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWUserInterface data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, DataArgumentType.UserInterface, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, DataArgumentType.UserInterface, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, DataArgumentType.UserInterface, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWCieColor data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Image, DataArgumentType.CieColor, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Image, DataArgumentType.CieColor, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Image, DataArgumentType.CieColor, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWExtImageInfo data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Image, DataArgumentType.ExtImageInfo, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Image, DataArgumentType.ExtImageInfo, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Image, DataArgumentType.ExtImageInfo, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWFilter data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Image, DataArgumentType.Filter, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Image, DataArgumentType.Filter, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Image, DataArgumentType.Filter, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWGrayResponse data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Image, DataArgumentType.GrayResponse, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Image, DataArgumentType.GrayResponse, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Image, DataArgumentType.GrayResponse, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWImageInfo data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Image, DataArgumentType.ImageInfo, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Image, DataArgumentType.ImageInfo, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Image, DataArgumentType.ImageInfo, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWImageLayout data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Image, DataArgumentType.ImageLayout, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Image, DataArgumentType.ImageLayout, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Image, DataArgumentType.ImageLayout, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWImageMemXfer data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Image, DataArgumentType.ImageMemXfer, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Image, DataArgumentType.ImageMemXfer, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Image, DataArgumentType.ImageMemXfer, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWJpegCompression data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Image, DataArgumentType.JpegCompression, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Image, DataArgumentType.JpegCompression, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Image, DataArgumentType.JpegCompression, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWPalette8 data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Image, DataArgumentType.Palette8, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Image, DataArgumentType.Palette8, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Image, DataArgumentType.Palette8, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWRgbResponse data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Image, DataArgumentType.RgbResponse, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Image, DataArgumentType.RgbResponse, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Image, DataArgumentType.RgbResponse, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, NTwain.Data.Message msg, TWStatus data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, DataArgumentType.Status, msg, data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, DataArgumentType.Status, msg, data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, DataArgumentType.Status, msg, data);
			}
			throw new PlatformNotSupportedException();
		}

		public static ReturnCode DsmEntry(TWIdentity origin, TWIdentity destination, DataArgumentType dat, NTwain.Data.Message msg, ref TWMemory data)
		{
			if (PlatformInfo.Current.IsWindows)
			{
				if (PlatformInfo.Current.UseNewWinDSM)
				{
					return NativeMethods.DsmWinNew(origin, destination, DataGroups.Control, dat, msg, ref data);
				}
				return NativeMethods.DsmWinOld(origin, destination, DataGroups.Control, dat, msg, ref data);
			}
			if (PlatformInfo.Current.IsLinux)
			{
				return NativeMethods.DsmLinux(origin, destination, DataGroups.Control, dat, msg, ref data);
			}
			throw new PlatformNotSupportedException();
		}
	}
	/// <summary>
	/// Base class for grouping triplet operations messages.
	/// </summary>
	public abstract class TripletBase
	{
		/// <summary>
		/// Gets the twain session.
		/// </summary>
		/// <value>
		/// The session.
		/// </value>
		internal ITwainSessionInternal Session { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Triplets.TripletBase" /> class.
		/// </summary>
		/// <param name="session">The session.</param>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		internal TripletBase(ITwainSessionInternal session)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			Session = session;
		}
	}
}
namespace NTwain.Properties
{
	/// <summary>
	///   A strongly-typed resource class, for looking up localized strings, etc.
	/// </summary>
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		/// <summary>
		///   Returns the cached ResourceManager instance used by this class.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					resourceMan = new ResourceManager("NTwain.Properties.Resources", typeof(Resources).Assembly);
				}
				return resourceMan;
			}
		}

		/// <summary>
		///   Overrides the current thread's CurrentUICulture property for all
		///   resource lookups using this strongly typed resource class.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		/// <summary>
		///   Looks up a localized string similar to Data group for application must contain either Image or Audio..
		/// </summary>
		internal static string BadDataGroupsForAppId => ResourceManager.GetString("BadDataGroupsForAppId", resourceCulture);

		/// <summary>
		///   Looks up a localized string similar to Invalid value type for {0}..
		/// </summary>
		internal static string BadValueType => ResourceManager.GetString("BadValueType", resourceCulture);

		/// <summary>
		///   Looks up a localized string similar to Capability {0} has invalid container type {1}..
		/// </summary>
		internal static string CapHasBadContainer => ResourceManager.GetString("CapHasBadContainer", resourceCulture);

		/// <summary>
		///   Looks up a localized string similar to The string value has exceeded the maximum length allowed..
		/// </summary>
		internal static string MaxStringLengthExceeded => ResourceManager.GetString("MaxStringLengthExceeded", resourceCulture);

		/// <summary>
		///   Looks up a localized string similar to Message loop has not started yet..
		/// </summary>
		internal static string MsgLoopUnavailble => ResourceManager.GetString("MsgLoopUnavailble", resourceCulture);

		/// <summary>
		///   Looks up a localized string similar to Source name is required..
		/// </summary>
		internal static string SourceRequired => ResourceManager.GetString("SourceRequired", resourceCulture);

		internal Resources()
		{
		}
	}
}
namespace NTwain.Interop
{
	/// <summary>
	/// Defines the dimensions and color information for a DIB.
	/// </summary>
	internal struct BITMAPINFO
	{
		/// <summary>
		/// Structure that contains information about the dimensions of color format.
		/// </summary>
		public BITMAPINFOHEADER bmiHeader;

		/// <summary>
		/// This contains one of the following:
		/// 1. An array of RGBQUAD. The elements of the array that make up the color table.
		/// 2. An array of 16-bit unsigned integers that specifies indexes into the currently realized logical palette. This use of bmiColors is allowed for functions that use DIBs.
		/// The number of entries in the array depends on the values of the biBitCount and biClrUsed members of the BITMAPINFOHEADER structure.
		/// </summary>
		public IntPtr bmiColors;
	}
	/// <summary>
	/// Structure that contains information about the dimensions and color format of a DIB.
	/// </summary>
	internal struct BITMAPINFOHEADER
	{
		/// <summary>
		/// Indicates the bitmap compression of <seealso cref="T:NTwain.Interop.BITMAPINFOHEADER" />.
		/// </summary>
		public enum CompressionType : uint
		{
			/// <summary>
			/// An uncompressed format.
			/// </summary>
			BI_RGB,
			/// <summary>
			///  A run-length encoded (RLE) format for bitmaps with 8 bpp. The compression format is a 2-byte format consisting of a count byte followed by a byte containing a color index. For more information, see Bitmap Compression. 
			/// </summary>
			BI_RLE8,
			/// <summary>
			///  An RLE, format for bitmaps with 4 bpp. The compression format is a 2-byte format consisting of a count byte followed by two word-length color indexes. For more information, see Bitmap Compression. 
			/// </summary>
			BI_RLE4,
			/// <summary>
			/// Specifies that the bitmap is not compressed and that the color table consists of three DWORD color masks that specify the red, green, and blue components of each pixel. 
			/// This is valid when used with 16- and 32-bpp bitmaps. 
			/// </summary>
			BI_BITFIELDS,
			/// <summary>
			/// Indicates that the image is a JPEG image.
			/// </summary>
			BI_JPEG,
			/// <summary>
			/// Indicates that the image is a PNG image.
			/// </summary>
			BI_PNG
		}

		/// <summary>
		/// The number of bytes required by the structure.
		/// </summary>
		public uint biSize;

		/// <summary>
		/// The width of the bitmap, in pixels.
		/// If Compression is JPEG or PNG, the Width member specifies the width of the decompressed 
		/// JPEG or PNG image file, respectively.
		/// </summary>
		public int biWidth;

		/// <summary>
		/// The height of the bitmap, in pixels. If Height is positive, 
		/// the bitmap is a bottom-up DIB and its origin is the lower-left corner. 
		/// If Height is negative, the bitmap is a top-down DIB and its origin is the upper-left corner.
		/// If Height is negative, indicating a top-down DIB, Compression must be either RGB or BITFIELDS. Top-down DIBs cannot be compressed.
		/// If Compression is JPEG or PNG, the Height member specifies the height of the decompressed JPEG or PNG image file, respectively.
		/// </summary>
		public int biHeight;

		/// <summary>
		/// The number of planes for the target device. This value must be set to 1.
		/// </summary>
		public ushort biPlanes;

		/// <summary>
		/// The number of bits-per-pixel. The BitCount member 
		/// determines the number of bits that define each pixel and the maximum number of colors in the bitmap. 
		/// </summary>
		public ushort biBitCount;

		/// <summary>
		/// The type of compression for a compressed bottom-up bitmap (top-down DIBs cannot be compressed).
		/// </summary>
		public CompressionType biCompression;

		/// <summary>
		/// The size, in bytes, of the image. This may be set to zero for RGB bitmaps.
		/// If Compression is JPEG or PNG, SizeImage indicates the size of the JPEG or PNG image buffer, respectively.
		/// </summary>
		public uint biSizeImage;

		/// <summary>
		/// The horizontal resolution, in pixels-per-meter, of the target device for the bitmap. 
		/// An application can use this value to select a bitmap from a resource group that 
		/// best matches the characteristics of the current device.
		/// </summary>
		public int biXPelsPerMeter;

		/// <summary>
		/// The vertical resolution, in pixels-per-meter, of the target device for the bitmap.
		/// </summary>
		public int biYPelsPerMeter;

		/// <summary>
		/// The number of color indexes in the color table that are actually used by the bitmap. 
		/// If this value is zero, the bitmap uses the maximum number of colors corresponding to 
		/// the value of the BitCount member for the compression mode specified by Compression.
		/// </summary>
		public uint biClrUsed;

		/// <summary>
		/// The number of color indexes that are required for displaying the bitmap. 
		/// If this value is zero, all colors are required.
		/// </summary>
		public uint biClrImportant;

		private const double METER_INCH_RATIO = 39.3700787;

		/// <summary>
		/// Gets whether the bitmap is bottom-up or top-down format.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is bottom up image; otherwise, <c>false</c>.
		/// </value>
		/// <returns></returns>
		public bool IsBottomUpImage => biHeight > 0;

		/// <summary>
		/// Gets the horizontal dpi of the bitmap.
		/// </summary>
		/// <returns></returns>
		public float GetXDpi()
		{
			return (float)Math.Round((double)biXPelsPerMeter / 39.3700787, 0);
		}

		/// <summary>
		/// Gets the vertical dpi of the bitmap.
		/// </summary>
		/// <returns></returns>
		public float GetYDpi()
		{
			return (float)Math.Round((double)biYPelsPerMeter / 39.3700787, 0);
		}

		/// <summary>
		/// Gets the size of the structure.
		/// </summary>
		/// <returns></returns>
		public static uint GetByteSize()
		{
			return (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
		}

		/// <summary>
		/// Checks to see if this structure contain valid data.
		/// It also fills in any missing pieces if possible.
		/// </summary>
		/// <returns></returns>
		public bool Validate()
		{
			if (biHeight != 0 && biWidth != 0 && biBitCount != 0)
			{
				if (biSize == 0)
				{
					biSize = GetByteSize();
				}
				if (biClrUsed == 0)
				{
					switch (biBitCount)
					{
					case 1:
						biClrUsed = 2u;
						break;
					case 4:
						biClrUsed = 16u;
						break;
					case 8:
						biClrUsed = 256u;
						break;
					}
				}
				if (biSizeImage == 0)
				{
					biSizeImage = (uint)((((biWidth * biBitCount + 31) & -32) >> 3) * Math.Abs(biHeight));
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets the pointer to scan0 given the header pointer.
		/// </summary>
		/// <param name="headerPtr">The header PTR.</param>
		/// <returns></returns>
		public IntPtr GetScan0(IntPtr headerPtr)
		{
			int num = (int)biClrUsed;
			if (num == 0 && biBitCount <= 8)
			{
				num = 1 << (int)biBitCount;
			}
			num = num * 4 + (int)biSize + headerPtr.ToInt32();
			return new IntPtr(num);
		}

		/// <summary>
		/// Gets the System.Drawing pixel format of current structure.
		/// </summary>
		/// <returns></returns>
		public PixelFormat GetDrawingPixelFormat()
		{
			return biBitCount switch
			{
				1 => PixelFormat.Format1bppIndexed, 
				4 => PixelFormat.Format4bppIndexed, 
				8 => PixelFormat.Format8bppIndexed, 
				16 => PixelFormat.Format16bppRgb565, 
				24 => PixelFormat.Format24bppRgb, 
				32 => PixelFormat.Format32bppRgb, 
				48 => PixelFormat.Format48bppRgb, 
				_ => PixelFormat.Undefined, 
			};
		}

		/// <summary>
		/// Gets the color palette that's contained in the header.
		/// Note not all images will have palette, so check if the return value
		/// is null before using it.
		/// </summary>
		/// <returns></returns>
		public ColorPalette GetDrawingPalette(IntPtr headerPtr)
		{
			if (biClrUsed != 0)
			{
				byte[] array = new byte[biClrUsed * 4];
				Marshal.Copy(new IntPtr(headerPtr.ToInt32() + biSize), array, 0, array.Length);
				Bitmap bitmap = new Bitmap(1, 1, GetDrawingPixelFormat());
				ColorPalette palette = bitmap.Palette;
				bitmap.Dispose();
				int num = 0;
				int num2 = array.Length / 4;
				for (int i = 0; i < num2; i++)
				{
					num = i * 4;
					palette.Entries[i] = Color.FromArgb(array[num + 2], array[num + 1], array[num]);
				}
				return palette;
			}
			return null;
		}

		/// <summary>
		/// Gets the stride size of this bitmap.
		/// </summary>
		/// <returns></returns>
		public int GetStride()
		{
			int num = biBitCount * biWidth;
			int num2 = num / 8 + ((num % 8 != 0) ? 1 : 0);
			int num3 = num2 % 4;
			if (num3 > 0)
			{
				num2 += 4 - num3;
			}
			return num2;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return new StringBuilder().Append("BitmapInfoHeader:").Append("\r\n\tSize = " + biSize).Append("\r\n\tWidth = " + biWidth)
				.Append("\r\n\tHeight = " + biHeight)
				.Append("\r\n\tPlanes = " + biPlanes)
				.Append("\r\n\tBitCount = " + biBitCount)
				.Append("\r\n\tCompression = " + biCompression)
				.Append("\r\n\tSizeImage = " + biSizeImage)
				.Append("\r\n\tXPixelsPerMeter = " + biXPelsPerMeter)
				.Append("\r\n\tYPixelsPerMeter = " + biYPelsPerMeter)
				.Append("\r\n\tColorUsed = " + biClrUsed)
				.Append("\r\n\tColorImportant = " + biClrImportant)
				.ToString();
		}
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct BITMAPFILEHEADER
	{
		public ushort bfType;

		public uint bfSize;

		public ushort bfReserved1;

		public ushort bfReserved2;

		public uint bfOffBits;
	}
	/// <summary>
	/// The MSG structure in Windows for TWAIN use.
	/// </summary>
	internal struct MESSAGE(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam)
	{
		private IntPtr _hwnd = hwnd;

		private uint _message = (uint)message;

		private IntPtr _wParam = wParam;

		private IntPtr _lParam = lParam;

		private uint _time = 0u;

		private int _x = 0;

		private int _y = 0;
	}
	internal static class NativeMethods
	{
		[DllImport("gdi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject(IntPtr hObject);

		[DllImport("user32.dll")]
		public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
	}
	/// <summary>
	/// The TIFF file header.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct TIFFHEADER
	{
		public ushort u8ByteOrder;

		public ushort u16Version;

		public uint u32OffsetFirstIFD;

		public ushort u16u16IFD;
	}
	/// <summary>
	/// An individual TIFF Tag.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct TIFFTAG
	{
		public ushort u16Tag;

		public ushort u16Type;

		public uint u32Count;

		public uint u32Value;
	}
	[SuppressUnmanagedCodeSecurity]
	internal static class UnsafeNativeMethods
	{
		[DllImport("kernel32", EntryPoint = "GlobalAlloc", SetLastError = true)]
		internal static extern IntPtr WinGlobalAlloc(uint uFlags, UIntPtr dwBytes);

		[DllImport("kernel32", EntryPoint = "GlobalFree", SetLastError = true)]
		internal static extern IntPtr WinGlobalFree(IntPtr hMem);

		[DllImport("kernel32", EntryPoint = "GlobalLock", SetLastError = true)]
		internal static extern IntPtr WinGlobalLock(IntPtr handle);

		[DllImport("kernel32", EntryPoint = "GlobalUnlock", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WinGlobalUnlock(IntPtr handle);
	}
}
namespace NTwain.Internals
{
	internal static class Extensions
	{
		/// <summary>
		/// Verifies the string length is under the maximum length
		/// and throws <see cref="T:System.ArgumentException" /> if violated.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="maxLength">The maximum length.</param>
		public static void VerifyLengthUnder(this string value, int maxLength)
		{
			if (value != null && value.Length > maxLength)
			{
				throw new ArgumentException(Resources.MaxStringLengthExceeded);
			}
		}

		/// <summary>
		/// Verifies the session is within the specified state range (inclusive). Throws
		/// <see cref="T:NTwain.TwainStateException" /> if violated.
		/// </summary>
		/// <param name="session">The session.</param>
		/// <param name="allowedMinimum">The allowed minimum.</param>
		/// <param name="allowedMaximum">The allowed maximum.</param>
		/// <param name="group">The triplet data group.</param>
		/// <param name="dataArgumentType">The triplet data argument type.</param>
		/// <param name="message">The triplet message.</param>
		/// <exception cref="T:NTwain.TwainStateException"></exception>
		public static void VerifyState(this ITwainSessionInternal session, int allowedMinimum, int allowedMaximum, DataGroups group, DataArgumentType dataArgumentType, NTwain.Data.Message message)
		{
			if (session.EnforceState && (session.State < allowedMinimum || session.State > allowedMaximum))
			{
				throw new TwainStateException(session.State, allowedMinimum, allowedMaximum, group, dataArgumentType, message, string.Format(CultureInfo.InvariantCulture, "TWAIN state {0} does not match required range {1}-{2} for operation {3}-{4}-{5}.", session.State, allowedMinimum, allowedMaximum, group, dataArgumentType, message));
			}
		}

		public static string ChangeExtensionByFormat(this TWSetupFileXfer fileInfo, string currentFilePath)
		{
			string text = null;
			switch (fileInfo.Format)
			{
			case FileFormat.Bmp:
				return Path.ChangeExtension(currentFilePath, ".bmp");
			case FileFormat.Dejavu:
				return Path.ChangeExtension(currentFilePath, ".dejavu");
			case FileFormat.Exif:
				return Path.ChangeExtension(currentFilePath, ".exit");
			case FileFormat.Fpx:
				return Path.ChangeExtension(currentFilePath, ".fpx");
			case FileFormat.Jfif:
				return Path.ChangeExtension(currentFilePath, ".jpg");
			case FileFormat.Jp2:
				return Path.ChangeExtension(currentFilePath, ".jp2");
			case FileFormat.Jpx:
				return Path.ChangeExtension(currentFilePath, ".jpx");
			case FileFormat.Pdf:
			case FileFormat.PdfA:
			case FileFormat.PdfA2:
				return Path.ChangeExtension(currentFilePath, ".pdf");
			case FileFormat.Pict:
				return Path.ChangeExtension(currentFilePath, ".pict");
			case FileFormat.Png:
				return Path.ChangeExtension(currentFilePath, ".png");
			case FileFormat.Spiff:
				return Path.ChangeExtension(currentFilePath, ".spiff");
			case FileFormat.Tiff:
			case FileFormat.TiffMulti:
				return Path.ChangeExtension(currentFilePath, ".tif");
			case FileFormat.Xbm:
				return Path.ChangeExtension(currentFilePath, ".xbm");
			default:
				return Path.ChangeExtension(currentFilePath, ".unknown");
			}
		}
	}
	/// <summary>
	/// For something that is in a pending state until finalized with a Commit() call.
	/// The changes are rolled back if it is disposed without being committed.
	/// </summary>
	internal interface ICommittable : IDisposable
	{
		/// <summary>
		/// Commits the pending changes.
		/// </summary>
		void Commit();
	}
	internal static class ImageTools
	{
		public static bool IsDib(IntPtr data)
		{
			return Marshal.ReadInt16(data) == BITMAPINFOHEADER.GetByteSize();
		}

		public static bool IsTiff(IntPtr data)
		{
			return Marshal.ReadInt16(data) == 18761;
		}

		public static Stream GetBitmapStream(IntPtr data)
		{
			BITMAPINFOHEADER bITMAPINFOHEADER = (BITMAPINFOHEADER)Marshal.PtrToStructure(data, typeof(BITMAPINFOHEADER));
			if (bITMAPINFOHEADER.Validate())
			{
				int num = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
				BITMAPFILEHEADER structure = default;
				structure.bfType = 19778;
				structure.bfOffBits = (uint)(num + (int)bITMAPINFOHEADER.biSize) + bITMAPINFOHEADER.biClrUsed * 4;
				structure.bfSize = structure.bfOffBits + bITMAPINFOHEADER.biSizeImage;
				byte[] array = new byte[structure.bfSize];
				IntPtr intPtr = Marshal.AllocHGlobal(num);
				Marshal.StructureToPtr(structure, intPtr, fDeleteOld: true);
				Marshal.Copy(intPtr, array, 0, num);
				Marshal.FreeHGlobal(intPtr);
				Marshal.Copy(data, array, num, (int)structure.bfSize - num);
				return new MemoryStream(array);
			}
			return null;
		}

		public static Stream GetTiffStream(IntPtr data)
		{
			int num = Marshal.SizeOf(typeof(TIFFHEADER));
			int num2 = Marshal.SizeOf(typeof(TIFFTAG));
			int num3 = 0;
			long num4 = data.ToInt64() + num;
			for (int i = 0; i < 999; i++)
			{
				num4 += num2 * i;
				TIFFTAG tIFFTAG = (TIFFTAG)Marshal.PtrToStructure((IntPtr)num4, typeof(TIFFTAG));
				ushort u16Tag = tIFFTAG.u16Tag;
				if (u16Tag == 273 || u16Tag == 279)
				{
					num3 += (int)tIFFTAG.u32Value;
				}
			}
			if (num3 > 0)
			{
				byte[] array = new byte[num3];
				Marshal.Copy(data, array, 0, num3);
				return new MemoryStream(array);
			}
			return null;
		}
	}
	/// <summary>
	/// This is the self-hosted message loop for TWAIN communication.
	/// It utilizes the wpf Dispatcher to do all the hard work.
	/// </summary>
	internal sealed class InternalMessageLoopHook : MessageLoopHook
	{
		private Dispatcher _dispatcher;

		private WindowsHook _hook;

		protected internal override void Stop()
		{
			if (_dispatcher != null)
			{
				_dispatcher.InvokeShutdown();
			}
		}

		protected internal override void Start(IWinMessageFilter filter)
		{
			if (_dispatcher != null)
			{
				return;
			}
			WrappedManualResetEvent hack = new WrappedManualResetEvent();
			try
			{
				Thread thread = new Thread(() =>
				{
					PlatformInfo.Current.Log.Debug("NTwain internal message loop is starting.");
					_dispatcher = Dispatcher.CurrentDispatcher;
					if (!PlatformInfo.Current.IsOnMono)
					{
						_hook = new WindowsHook(filter);
						Handle = _hook.Handle;
					}
					hack.Set();
					Dispatcher.Run();
					_dispatcher = null;
					if (_hook != null)
					{
						_hook.Dispose();
						_hook = null;
						Handle = IntPtr.Zero;
					}
				});
				thread.IsBackground = true;
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				hack.Wait();
			}
			finally
			{
				if (hack != null)
				{
					((IDisposable)hack).Dispose();
				}
			}
		}

		public override void BeginInvoke(Action action)
		{
			if (_dispatcher == null)
			{
				throw new InvalidOperationException(Resources.MsgLoopUnavailble);
			}
			_dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
		}

		public override void Invoke(Action action)
		{
			if (_dispatcher == null)
			{
				throw new InvalidOperationException(Resources.MsgLoopUnavailble);
			}
			Exception error = null;
			if (_dispatcher.CheckAccess())
			{
				action();
			}
			else if (PlatformInfo.Current.IsOnMono)
			{
				WrappedManualResetEvent man = new WrappedManualResetEvent();
				try
				{
					_dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
					{
						try
						{
							action();
						}
						catch (Exception ex)
						{
							error = ex;
						}
						finally
						{
							man.Set();
						}
					}));
					man.Wait();
				}
				finally
				{
					if (man != null)
					{
						((IDisposable)man).Dispose();
					}
				}
			}
			else
			{
				_dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
				{
					try
					{
						action();
					}
					catch (Exception ex)
					{
						error = ex;
					}
				}));
			}
			if (error != null)
			{
				MessageLoopHook.Rethrow(error);
			}
		}
	}
	/// <summary>
	/// Extends <see cref="T:NTwain.ITwainSession" /> with extra stuff for internal use.
	/// </summary>
	internal interface ITwainSessionInternal : ITwainSession, IEnumerable<DataSource>, IEnumerable, INotifyPropertyChanged, ITripletControl
	{
		/// <summary>
		/// Gets the app id used for the session.
		/// </summary>
		/// <returns></returns>
		TWIdentity AppId { get; }

		MessageLoopHook MessageLoopHook { get; }

		bool CloseDSRequested { get; }

		/// <summary>
		/// Gets the triplet operations defined for audio data group.
		/// </summary>
		DGAudio DGAudio { get; }

		/// <summary>
		/// Changes the state right away.
		/// </summary>
		/// <param name="newState">The new state.</param>
		/// <param name="notifyChange">if set to <c>true</c> to notify change.</param>
		void ChangeState(int newState, bool notifyChange);

		/// <summary>
		/// Gets the pending state changer and tentatively changes the session state to the specified value.
		/// Value will only stick if committed.
		/// </summary>
		/// <param name="newState">The new state.</param>
		/// <returns></returns>
		ICommittable GetPendingStateChanger(int newState);

		void ChangeCurrentSource(DataSource source);

		void UpdateCallback();

		ReturnCode DisableSource();

		void SafeSyncableRaiseEvent(DataTransferredEventArgs e);

		void SafeSyncableRaiseEvent(TransferCanceledEventArgs e);

		void SafeSyncableRaiseEvent(TransferErrorEventArgs e);

		void SafeSyncableRaiseEvent(TransferReadyEventArgs e);

		ReturnCode EnableSource(SourceEnableMode mode, bool modal, IntPtr windowHandle);
	}
	internal class LinuxMemoryManager : IMemoryManager
	{
		public IntPtr Allocate(uint size)
		{
			return Marshal.AllocHGlobal((int)size);
		}

		public void Free(IntPtr handle)
		{
			Marshal.FreeHGlobal(handle);
		}

		public IntPtr Lock(IntPtr handle)
		{
			return handle;
		}

		public void Unlock(IntPtr handle)
		{
		}
	}
	internal class TentativeStateCommitable : ICommittable, IDisposable
	{
		private bool _commit;

		private ITwainSessionInternal _session;

		private int _origState;

		private int _newState;

		public TentativeStateCommitable(ITwainSessionInternal session, int newState)
		{
			_session = session;
			_origState = session.State;
			_newState = newState;
			_session.ChangeState(newState, notifyChange: false);
		}

		public void Commit()
		{
			if (_session.State == _newState)
			{
				_session.ChangeState(_newState, notifyChange: true);
			}
			_commit = true;
		}

		public void Dispose()
		{
			if (!_commit && _session.State == _newState)
			{
				_session.ChangeState(_origState, notifyChange: false);
			}
		}
	}
	internal class TraceLog : ILog
	{
		public bool IsInfoEnabled { get; set; }

		public bool IsDebugEnabled { get; set; }

		public bool IsErrorEnabled { get; set; }

		public TraceLog()
		{
			IsInfoEnabled = true;
			IsErrorEnabled = true;
		}

		public void Info(string message)
		{
			if (IsInfoEnabled && message != null)
			{
				Trace.WriteLine(message, "Info");
			}
		}

		public void Info(string messageFormat, params object[] args)
		{
			Debug(string.Format(CultureInfo.CurrentCulture, messageFormat, args));
		}

		public void Debug(string message)
		{
			if (IsDebugEnabled && message != null)
			{
				Trace.WriteLine(message, "Debug");
			}
		}

		public void Debug(string messageFormat, params object[] args)
		{
			Debug(string.Format(CultureInfo.CurrentCulture, messageFormat, args));
		}

		public void Error(string message)
		{
			if (IsErrorEnabled && message != null)
			{
				Trace.WriteLine(message, "Error");
			}
		}

		public void Error(string message, Exception exception)
		{
			if (exception == null)
			{
				Error(message);
			}
			else
			{
				Error(message + Environment.NewLine + exception.ToString());
			}
		}

		public void Error(string messageFormat, Exception exception, params object[] args)
		{
			if (exception == null)
			{
				Error(string.Format(CultureInfo.CurrentCulture, messageFormat, args));
			}
			else
			{
				Error(string.Format(CultureInfo.CurrentCulture, messageFormat, args) + Environment.NewLine + exception.ToString());
			}
		}
	}
	/// <summary>
	/// Contains the actual data transfer logic since TwainSession is getting too large.
	/// </summary>
	internal static class TransferLogic
	{
		/// <summary>
		/// Performs the TWAIN transfer routine at state 6. 
		/// </summary>
		public static void DoTransferRoutine(ITwainSessionInternal session)
		{
			bool flag = true;
			bool flag2 = false;
			DataGroups value = DataGroups.None;
			XferMech xferMech = XferMech.Native;
			XferMech xferMech2 = XferMech.Native;
			if (session.DGControl.XferGroup.Get(ref value) == ReturnCode.Success)
			{
				flag2 = (value & DataGroups.Audio) == DataGroups.Audio;
				flag = (session.CurrentSource.Identity.ProductName.IndexOf("Plustek", StringComparison.OrdinalIgnoreCase) > -1 && session.CurrentSource.Identity.ProductName.IndexOf("OpticSlim", StringComparison.OrdinalIgnoreCase) > -1 && session.CurrentSource.Identity.ProductName.IndexOf("2680H", StringComparison.OrdinalIgnoreCase) > -1) || value == DataGroups.None || (value & DataGroups.Image) == DataGroups.Image;
			}
			if (flag)
			{
				xferMech = session.CurrentSource.Capabilities.ICapXferMech.GetCurrent();
			}
			if (flag2)
			{
				session.CurrentSource.Capabilities.ACapXferMech.GetCurrent();
			}
			// Проба для Kyocera: на этих драйверах первая четвёрка-триплет после
			// XferReady (DAT_PENDINGXFERS/MSG_GET) висит навсегда (внешние снимки
			// стеков 1.7.19: помпа сидит в PendingXfers.Get на 25/60/100/140-й
			// секунде). DAT_IMAGEINFO MSG_GET спек-легален в состоянии 6,
			// читается без последствий — а по опыту twain-dsm #23 некоторые
			// нули-драйверы ждут его первым звонком. Проба включается только для
			// KYOCERA, чтобы не беспокоить здоровые драйверы других машин.
			string probeName = session.CurrentSource.Identity.Manufacturer ?? string.Empty;
			string probeDevice = session.CurrentSource.Identity.ProductName ?? string.Empty;
			if (probeName.IndexOf("KYOCERA", StringComparison.OrdinalIgnoreCase) >= 0
				|| probeDevice.IndexOf("KYOCERA", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				PlatformInfo.Current.Log.Debug("NAPS2.TW - Kyocera probe: DAT_IMAGEINFO first");
				if (session.DGImage.ImageInfo.Get(out TWImageInfo probeInfo) == ReturnCode.Success)
				{
					PlatformInfo.Current.Log.Debug(
						"NAPS2.TW - DAT_IMAGEINFO ok: {0}x{1}, bpp {2}",
						probeInfo.ImageWidth, probeInfo.ImageLength, probeInfo.BitsPerPixel);
				}
				else
				{
					PlatformInfo.Current.Log.Debug("NAPS2.TW - DAT_IMAGEINFO failed");
				}
			}

			TWPendingXfers tWPendingXfers = new TWPendingXfers();
			ReturnCode returnCode = session.DGControl.PendingXfers.Get(tWPendingXfers);
			if (returnCode == ReturnCode.Success)
			{
				do
				{
					TransferReadyEventArgs e = new TransferReadyEventArgs(session.CurrentSource, tWPendingXfers.Count, tWPendingXfers.EndOfJob);
					session.SafeSyncableRaiseEvent(e);
					if (e.CancelAll || session.CloseDSRequested)
					{
						returnCode = session.DGControl.PendingXfers.Reset(tWPendingXfers);
						continue;
					}
					if (!e.CancelCurrent)
					{
						if (flag)
						{
							returnCode = xferMech switch
							{
								XferMech.Memory => DoImageMemoryXfer(session), 
								XferMech.File => DoImageFileXfer(session), 
								XferMech.MemFile => DoImageMemoryFileXfer(session), 
								_ => DoImageNativeXfer(session), 
							};
						}
						if (flag2)
						{
							returnCode = ((xferMech2 == XferMech.Native || xferMech2 != XferMech.File) ? DoAudioNativeXfer(session) : DoAudioFileXfer(session));
						}
					}
					if (returnCode != ReturnCode.Success && session.StopOnTransferError)
					{
						session.DGControl.PendingXfers.Reset(tWPendingXfers);
					}
					else
					{
						returnCode = session.DGControl.PendingXfers.EndXfer(tWPendingXfers);
					}
				}
				while (returnCode == ReturnCode.Success && tWPendingXfers.Count != 0 && !session.CloseDSRequested);
			}
			else
			{
				HandleReturnCode(session, returnCode);
			}
			if (session.State > 5)
			{
				session.ChangeState(5, notifyChange: true);
				session.DisableSource();
			}
		}

		private static void HandleReturnCode(ITwainSessionInternal session, ReturnCode rc)
		{
			switch (rc)
			{
			case ReturnCode.Cancel:
				session.SafeSyncableRaiseEvent(new TransferCanceledEventArgs());
				break;
			default:
			{
				TWStatus status = session.CurrentSource.GetStatus();
				session.SafeSyncableRaiseEvent(new TransferErrorEventArgs(rc, status));
				break;
			}
			case ReturnCode.Success:
			case ReturnCode.XferDone:
				break;
			}
		}

		private static ReturnCode DoAudioNativeXfer(ITwainSessionInternal session)
		{
			IntPtr handle = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			ReturnCode returnCode = ReturnCode.Failure;
			try
			{
				returnCode = session.DGAudio.AudioNativeXfer.Get(ref handle);
				if (returnCode == ReturnCode.XferDone)
				{
					session.ChangeState(7, notifyChange: true);
					if (handle != IntPtr.Zero)
					{
						intPtr = PlatformInfo.Current.MemoryManager.Lock(handle);
					}
					session.SafeSyncableRaiseEvent(new DataTransferredEventArgs(session.CurrentSource, intPtr));
				}
				else
				{
					HandleReturnCode(session, returnCode);
				}
			}
			catch (Exception error)
			{
				session.SafeSyncableRaiseEvent(new TransferErrorEventArgs(error));
			}
			finally
			{
				session.ChangeState(6, notifyChange: true);
				if (intPtr != IntPtr.Zero)
				{
					PlatformInfo.Current.MemoryManager.Unlock(handle);
					intPtr = IntPtr.Zero;
				}
				if (handle != IntPtr.Zero)
				{
					PlatformInfo.Current.MemoryManager.Free(handle);
					handle = IntPtr.Zero;
				}
			}
			return returnCode;
		}

		private static ReturnCode DoAudioFileXfer(ITwainSessionInternal session)
		{
			string text = null;
			ReturnCode returnCode = session.DGControl.SetupFileXfer.Get(out var setupFileXfer);
			if (returnCode == ReturnCode.Success)
			{
				text = setupFileXfer.FileName;
				returnCode = session.DGAudio.AudioFileXfer.Get();
				if (returnCode == ReturnCode.XferDone)
				{
					session.SafeSyncableRaiseEvent(new DataTransferredEventArgs(session.CurrentSource, text, FileFormat.Tiff));
				}
				else
				{
					HandleReturnCode(session, returnCode);
				}
			}
			return returnCode;
		}

		private static ReturnCode DoImageNativeXfer(ITwainSessionInternal session)
		{
			IntPtr handle = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			ReturnCode returnCode = ReturnCode.Failure;
			try
			{
				returnCode = session.DGImage.ImageNativeXfer.Get(ref handle);
				if (returnCode == ReturnCode.XferDone)
				{
					session.ChangeState(7, notifyChange: true);
					if (handle != IntPtr.Zero)
					{
						intPtr = PlatformInfo.Current.MemoryManager.Lock(handle);
					}
					DoImageXferredEventRoutine(session, intPtr, null, null, null, FileFormat.Tiff);
				}
				else
				{
					HandleReturnCode(session, returnCode);
				}
			}
			catch (Exception error)
			{
				session.SafeSyncableRaiseEvent(new TransferErrorEventArgs(error));
			}
			finally
			{
				session.ChangeState(6, notifyChange: true);
				if (intPtr != IntPtr.Zero)
				{
					PlatformInfo.Current.MemoryManager.Unlock(handle);
					intPtr = IntPtr.Zero;
				}
				if (handle != IntPtr.Zero)
				{
					PlatformInfo.Current.MemoryManager.Free(handle);
					handle = IntPtr.Zero;
				}
			}
			return returnCode;
		}

		private static ReturnCode DoImageFileXfer(ITwainSessionInternal session)
		{
			string filePath = null;
			if (session.DGControl.SetupFileXfer.Get(out var setupFileXfer) == ReturnCode.Success)
			{
				filePath = setupFileXfer.FileName;
			}
			ReturnCode returnCode = session.DGImage.ImageFileXfer.Get();
			if (returnCode == ReturnCode.XferDone)
			{
				DoImageXferredEventRoutine(session, IntPtr.Zero, null, null, filePath, setupFileXfer.Format);
			}
			else
			{
				HandleReturnCode(session, returnCode);
			}
			return returnCode;
		}

		private static ReturnCode DoImageMemoryXfer(ITwainSessionInternal session)
		{
			ReturnCode returnCode = session.DGControl.SetupMemXfer.Get(out var setupMemXfer);
			if (returnCode == ReturnCode.Success)
			{
				TWImageMemXfer tWImageMemXfer = new TWImageMemXfer();
				try
				{
					tWImageMemXfer.Memory = new TWMemory
					{
						Flags = (MemoryFlags.AppOwns | MemoryFlags.Pointer),
						Length = setupMemXfer.Preferred,
						TheMem = PlatformInfo.Current.MemoryManager.Allocate(setupMemXfer.Preferred)
					};
					do
					{
						returnCode = session.DGImage.ImageMemXfer.Get(tWImageMemXfer);
						if (returnCode != ReturnCode.Success && returnCode != ReturnCode.XferDone)
						{
							continue;
						}
						if (session.State != 7)
						{
							session.ChangeState(7, notifyChange: true);
						}
						byte[] array = new byte[tWImageMemXfer.BytesWritten];
						IntPtr intPtr = IntPtr.Zero;
						try
						{
							intPtr = PlatformInfo.Current.MemoryManager.Lock(tWImageMemXfer.Memory.TheMem);
							Marshal.Copy(intPtr, array, 0, array.Length);
						}
						finally
						{
							if (intPtr != IntPtr.Zero)
							{
								PlatformInfo.Current.MemoryManager.Unlock(tWImageMemXfer.Memory.TheMem);
							}
						}
						DoImageXferredEventRoutine(session, IntPtr.Zero, tWImageMemXfer, array, null, FileFormat.Tiff);
					}
					while (returnCode == ReturnCode.Success);
					HandleReturnCode(session, returnCode);
				}
				catch (Exception error)
				{
					session.SafeSyncableRaiseEvent(new TransferErrorEventArgs(error));
				}
				finally
				{
					session.ChangeState(6, notifyChange: true);
					if (tWImageMemXfer.Memory.TheMem != IntPtr.Zero)
					{
						PlatformInfo.Current.MemoryManager.Free(tWImageMemXfer.Memory.TheMem);
					}
				}
			}
			return returnCode;
		}

		private static ReturnCode DoImageMemoryFileXfer(ITwainSessionInternal session)
		{
			ReturnCode returnCode = ReturnCode.Failure;
			if (session.DGControl.SetupMemXfer.Get(out var setupMemXfer) == ReturnCode.Success && session.DGControl.SetupFileXfer.Get(out var setupFileXfer) == ReturnCode.Success)
			{
				TWImageMemXfer tWImageMemXfer = new TWImageMemXfer();
				string tempFileName = Path.GetTempFileName();
				string text = null;
				try
				{
					tWImageMemXfer.Memory = new TWMemory
					{
						Flags = (MemoryFlags.AppOwns | MemoryFlags.Pointer),
						Length = setupMemXfer.Preferred,
						TheMem = PlatformInfo.Current.MemoryManager.Allocate(setupMemXfer.Preferred)
					};
					returnCode = ReturnCode.Success;
					using (FileStream fileStream = File.OpenWrite(tempFileName))
					{
						do
						{
							returnCode = session.DGImage.ImageMemFileXfer.Get(tWImageMemXfer);
							if (returnCode != ReturnCode.Success && returnCode != ReturnCode.XferDone)
							{
								continue;
							}
							session.ChangeState(7, notifyChange: true);
							byte[] array = new byte[tWImageMemXfer.BytesWritten];
							IntPtr intPtr = IntPtr.Zero;
							try
							{
								intPtr = PlatformInfo.Current.MemoryManager.Lock(tWImageMemXfer.Memory.TheMem);
								Marshal.Copy(intPtr, array, 0, array.Length);
							}
							finally
							{
								if (intPtr != IntPtr.Zero)
								{
									PlatformInfo.Current.MemoryManager.Unlock(tWImageMemXfer.Memory.TheMem);
								}
							}
							fileStream.Write(array, 0, array.Length);
						}
						while (returnCode == ReturnCode.Success);
					}
					if (returnCode == ReturnCode.XferDone)
					{
						text = setupFileXfer.ChangeExtensionByFormat(tempFileName);
						File.Move(tempFileName, text);
					}
					else
					{
						HandleReturnCode(session, returnCode);
					}
				}
				catch (Exception error)
				{
					session.SafeSyncableRaiseEvent(new TransferErrorEventArgs(error));
				}
				finally
				{
					session.ChangeState(6, notifyChange: true);
					if (tWImageMemXfer.Memory.TheMem != IntPtr.Zero)
					{
						PlatformInfo.Current.MemoryManager.Free(tWImageMemXfer.Memory.TheMem);
					}
					if (File.Exists(tempFileName))
					{
						File.Delete(tempFileName);
					}
				}
				if (File.Exists(text))
				{
					DoImageXferredEventRoutine(session, IntPtr.Zero, null, null, text, setupFileXfer.Format);
				}
			}
			return returnCode;
		}

		private static void DoImageXferredEventRoutine(ITwainSessionInternal session, IntPtr dataPtr, TWImageMemXfer memInfo, byte[] memData, string filePath, FileFormat format)
		{
			DataTransferredEventArgs e = null;
			if (dataPtr != IntPtr.Zero)
			{
				e = new DataTransferredEventArgs(session.CurrentSource, dataPtr);
			}
			else
			{
				e = ((memData == null) ? new DataTransferredEventArgs(session.CurrentSource, filePath, format) : new DataTransferredEventArgs(session.CurrentSource, memInfo, memData));
			}
			session.SafeSyncableRaiseEvent(e);
		}
	}
	/// <summary>
	/// Abstracts out wnd proc hook on Windows from InternalMessageLoopHook class.
	/// This allows things to not depend on PresentationCore.dll at runtime on mono.
	/// </summary>
	internal class WindowsHook : IDisposable
	{
		private IDisposable _win;

		private MessageLoopHook.IWinMessageFilter _filter;

		public IntPtr Handle { get; private set; }

		public WindowsHook(MessageLoopHook.IWinMessageFilter filter)
		{
			_filter = filter;
			HwndSource hwndSource = null;
			try
			{
				hwndSource = new HwndSource(512, 13565952, 262144, 0, 0, "NTWAIN_LOOPER", IntPtr.Zero);
				Handle = hwndSource.Handle;
				hwndSource.AddHook(WndProc);
				_win = hwndSource;
			}
			catch
			{
				hwndSource?.Dispose();
				throw;
			}
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (_filter != null)
			{
				handled = _filter.IsTwainMessage(hwnd, msg, wParam, lParam);
			}
			if (!handled)
			{
				handled = true;
				return NativeMethods.DefWindowProc(hwnd, (uint)msg, wParam, lParam);
			}
			return IntPtr.Zero;
		}

		public void Dispose()
		{
			if (_win != null)
			{
				((HwndSource)_win).RemoveHook(WndProc);
				_win.Dispose();
				_win = null;
				Handle = IntPtr.Zero;
			}
		}
	}
	/// <summary>
	/// Provides methods for managing memory on data exchanged with twain sources using old win32 methods.
	/// </summary>
	internal class WinMemoryManager : IMemoryManager
	{
		public IntPtr Allocate(uint size)
		{
			IntPtr intPtr = UnsafeNativeMethods.WinGlobalAlloc(64u, new UIntPtr(size));
			if (intPtr == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			return intPtr;
		}

		public void Free(IntPtr handle)
		{
			UnsafeNativeMethods.WinGlobalFree(handle);
		}

		public IntPtr Lock(IntPtr handle)
		{
			return UnsafeNativeMethods.WinGlobalLock(handle);
		}

		public void Unlock(IntPtr handle)
		{
			UnsafeNativeMethods.WinGlobalUnlock(handle);
		}
	}
	internal class WrappedManualResetEvent : IDisposable
	{
		private ManualResetEventSlim _slim;

		public WrappedManualResetEvent()
		{
			_slim = new ManualResetEventSlim();
		}

		public void Wait()
		{
			_slim.Wait();
		}

		public void Set()
		{
			_slim.Set();
		}

		public void Dispose()
		{
			_slim.Dispose();
		}
	}
}
namespace NTwain.Data
{
	/// <summary>
	/// Stores a fixed point number. This can be implicitly converted 
	/// to a float in dotnet.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct TWFix32 : IEquatable<TWFix32>, IConvertible
	{
		private short _whole;

		private ushort _frac;

		/// <summary>
		/// The Whole part of the floating point number. This number is signed.
		/// </summary>
		public short Whole
		{
			get
			{
				return _whole;
			}
			set
			{
				_whole = value;
			}
		}

		/// <summary>
		/// The Fractional part of the floating point number. This number is unsigned.
		/// </summary>
		public ushort Fraction
		{
			get
			{
				return _frac;
			}
			set
			{
				_frac = value;
			}
		}

		private float ToFloat()
		{
			return (float)_whole + (float)(int)_frac / 65536f;
		}

		private TWFix32(float value)
		{
			bool flag = value < 0f;
			int num = (int)((double)value * 65536.0 + (flag ? (-0.5) : 0.5));
			_whole = (short)(num >> 16);
			_frac = (ushort)(num & 0xFFFF);
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return ToFloat().ToString(CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts this to <see cref="T:NTwain.Data.TWOneValue" /> for capability set methods.
		/// </summary>
		/// <returns></returns>
		public TWOneValue ToOneValue()
		{
			byte[] array = new byte[4];
			Buffer.BlockCopy(BitConverter.GetBytes(Whole), 0, array, 0, 2);
			Buffer.BlockCopy(BitConverter.GetBytes(Fraction), 0, array, 2, 2);
			uint item = BitConverter.ToUInt32(array, 0);
			return new TWOneValue
			{
				ItemType = ItemType.Fix32,
				Item = item
			};
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (!(obj is TWFix32))
			{
				return false;
			}
			return Equals((TWFix32)obj);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns></returns>
		public bool Equals(TWFix32 other)
		{
			if (_whole == other._whole)
			{
				return _frac == other._frac;
			}
			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return _whole ^ _frac;
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:NTwain.Data.TWFix32" /> to <see cref="T:System.Single" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator float(TWFix32 value)
		{
			return value.ToFloat();
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Single" /> to <see cref="T:NTwain.Data.TWFix32" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator TWFix32(float value)
		{
			return new TWFix32(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:NTwain.Data.TWFix32" /> to <see cref="T:System.Double" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator double(TWFix32 value)
		{
			return value.ToFloat();
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:System.Double" /> to <see cref="T:NTwain.Data.TWFix32" />.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator TWFix32(double value)
		{
			return new TWFix32((float)value);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="value1">The value1.</param>
		/// <param name="value2">The value2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(TWFix32 value1, TWFix32 value2)
		{
			return value1.Equals(value2);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="value1">The value1.</param>
		/// <param name="value2">The value2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(TWFix32 value1, TWFix32 value2)
		{
			return !value1.Equals(value2);
		}

		TypeCode IConvertible.GetTypeCode()
		{
			return TypeCode.Single;
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return this != 0f;
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(this);
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			return Convert.ToChar(this);
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			return Convert.ToDateTime(this);
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal(this);
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return Convert.ToDouble(this);
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(this);
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(this);
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(this);
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(this);
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return Convert.ToSingle(this);
		}

		string IConvertible.ToString(IFormatProvider provider)
		{
			return ToString();
		}

		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			return Convert.ChangeType((float)this, conversionType, CultureInfo.InvariantCulture);
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(this);
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(this);
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(this);
		}
	}
	/// <summary>
	/// Embedded in the <see cref="T:NTwain.Data.TWImageLayout" /> structure. 
	/// Defines a frame rectangle in ICapUnits coordinates.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct TWFrame : IEquatable<TWFrame>
	{
		private TWFix32 _left;

		private TWFix32 _top;

		private TWFix32 _right;

		private TWFix32 _bottom;

		/// <summary>
		/// Value of the left-most edge of the rectangle.
		/// </summary>
		public float Left
		{
			get
			{
				return _left;
			}
			set
			{
				_left = value;
			}
		}

		/// <summary>
		/// Value of the top-most edge of the rectangle.
		/// </summary>
		public float Top
		{
			get
			{
				return _top;
			}
			set
			{
				_top = value;
			}
		}

		/// <summary>
		/// Value of the right-most edge of the rectangle.
		/// </summary>
		public float Right
		{
			get
			{
				return _right;
			}
			set
			{
				_right = value;
			}
		}

		/// <summary>
		/// Value of the bottom-most edge of the rectangle.
		/// </summary>
		public float Bottom
		{
			get
			{
				return _bottom;
			}
			set
			{
				_bottom = value;
			}
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "L={0}, T={1}, R={2}, B={3}", Left, Top, Right, Bottom);
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (!(obj is TWFrame))
			{
				return false;
			}
			return Equals((TWFrame)obj);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns></returns>
		public bool Equals(TWFrame other)
		{
			if (_left == other._left && _top == other._top && _right == other._right)
			{
				return _bottom == other._bottom;
			}
			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return _left.GetHashCode() ^ _top.GetHashCode() ^ _right.GetHashCode() ^ _bottom.GetHashCode();
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="value1">The value1.</param>
		/// <param name="value2">The value2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(TWFrame value1, TWFrame value2)
		{
			return value1.Equals(value2);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="value1">The value1.</param>
		/// <param name="value2">The value2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(TWFrame value1, TWFrame value2)
		{
			return !value1.Equals(value2);
		}
	}
	/// <summary>
	/// Embedded in the <see cref="T:NTwain.Data.TWTransformStage" /> structure that is embedded in the <see cref="T:NTwain.Data.TWCieColor" />
	/// structure. Defines the parameters used for channel-specific transformation. The transform can be
	/// described either as an extended form of the gamma function or as a table look-up with linear
	/// interpolation.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct TWDecodeFunction : IEquatable<TWDecodeFunction>
	{
		private TWFix32 _startIn;

		private TWFix32 _breakIn;

		private TWFix32 _endIn;

		private TWFix32 _startOut;

		private TWFix32 _breakOut;

		private TWFix32 _endOut;

		private TWFix32 _gamma;

		private TWFix32 _sampleCount;

		/// <summary>
		/// Starting input value of the extended gamma function. Defines the
		/// minimum input value of channel data.
		/// </summary>
		public float StartIn => _startIn;

		/// <summary>
		/// Ending input value of the extended gamma function. Defines the maximum
		/// input value of channel data.
		/// </summary>
		public float BreakIn => _breakIn;

		/// <summary>
		/// The input value at which the transform switches from linear
		/// transformation/interpolation to gamma transformation.
		/// </summary>
		public float EndIn => _endIn;

		/// <summary>
		/// Starting output value of the extended gamma function. Defines the
		/// minimum output value of channel data.
		/// </summary>
		public float StartOut => _startOut;

		/// <summary>
		/// Ending output value of the extended gamma function. Defines the
		/// maximum output value of channel data.
		/// </summary>
		public float BreakOut => _breakOut;

		/// <summary>
		/// The output value at which the transform switches from linear
		/// transformation/interpolation to gamma transformation.
		/// </summary>
		public float EndOut => _endOut;

		/// <summary>
		/// Constant value. The exponential used in the gamma function.
		/// </summary>
		public float Gamma => _gamma;

		/// <summary>
		/// The number of samples in the look-up table. Includes the values of StartIn
		/// and EndIn. Zero-based index (actually, number of samples - 1). If zero, use
		/// extended gamma, otherwise use table look-up.
		/// </summary>
		public float SampleCount => _sampleCount;

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (!(obj is TWDecodeFunction))
			{
				return false;
			}
			return Equals((TWDecodeFunction)obj);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns></returns>
		public bool Equals(TWDecodeFunction other)
		{
			if (_startIn == other._startIn && _startOut == other._startOut && _breakIn == other._breakIn && _breakOut == other._breakOut && _endIn == other._endIn && _endOut == other._endOut && _gamma == other._gamma)
			{
				return _sampleCount == other._sampleCount;
			}
			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return _startIn.GetHashCode() ^ _startOut.GetHashCode() ^ _breakIn.GetHashCode() ^ _breakOut.GetHashCode() ^ _endIn.GetHashCode() ^ _endOut.GetHashCode() ^ _gamma.GetHashCode() ^ _sampleCount.GetHashCode();
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="value1">The value1.</param>
		/// <param name="value2">The value2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(TWDecodeFunction value1, TWDecodeFunction value2)
		{
			return value1.Equals(value2);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="value1">The value1.</param>
		/// <param name="value2">The value2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(TWDecodeFunction value1, TWDecodeFunction value2)
		{
			return !value1.Equals(value2);
		}
	}
	/// <summary>
	/// Specifies the parametrics used for either the ABC or LMN transform stages.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct TWTransformStage
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		private TWDecodeFunction[] _decode;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
		private TWFix32[] _mix;

		/// <summary>
		/// Channel-specific transform parameters.
		/// </summary>
		public TWDecodeFunction[] Decode => _decode;

		/// <summary>
		/// Flattened 3x3 matrix that specifies how channels are mixed in.
		/// </summary>
		public TWFix32[] Mix => _mix;

		/// <summary>
		/// Gets the <see cref="P:NTwain.Data.TWTransformStage.Mix" /> value as matrix.
		/// </summary>
		/// <returns></returns>
		public TWFix32[,] GetMixMatrix()
		{
			TWFix32[,] array = new TWFix32[3, 3];
			Buffer.BlockCopy(_mix, 0, array, 0, _mix.Length * 4);
			return array;
		}
	}
	/// <summary>
	/// Stores a group of associated individual values for a capability.
	/// The values need have no relationship to one another aside from 
	/// being used to describe the same "value" of the capability
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWArray
	{
		private ushort _itemType;

		private uint _numItems;

		private object[] _itemList;

		/// <summary>
		/// The type of items in the array. All items in the array have the same size.
		/// </summary>
		public ItemType ItemType
		{
			get
			{
				return (ItemType)_itemType;
			}
			set
			{
				_itemType = (ushort)value;
			}
		}

		/// <summary>
		/// Array of ItemType values starts here.
		/// </summary>
		public object[] ItemList
		{
			get
			{
				return _itemList;
			}
			set
			{
				_itemList = value;
				if (value != null)
				{
					_numItems = (uint)value.Length;
				}
				else
				{
					_numItems = 0u;
				}
			}
		}
	}
	/// <summary>
	/// Used to get audio info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	[BestFitMapping(false, ThrowOnUnmappableChar = true)]
	public class TWAudioInfo
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		private string _name;

		private uint _reserved;

		/// <summary>
		/// Name of audio data.
		/// </summary>
		public string Name => _name;

		internal TWAudioInfo()
		{
		}
	}
	/// <summary>
	/// Used in Callback mechanism for sending messages from the Source to the Application.
	/// Applications version 2.2 or higher must use <see cref="T:NTwain.Data.TWCallback2" />.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal class TWCallback
	{
		[MarshalAs(UnmanagedType.FunctionPtr)]
		private CallbackDelegate _callBackProc;

		private uint _refCon;

		private short _message;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWCallback" /> class.
		/// </summary>
		/// <param name="callback">The callback function’s entry point.</param>
		public TWCallback(CallbackDelegate callback)
		{
			_callBackProc = callback;
		}
	}
	/// <summary>
	/// Used in the Callback mechanism for sending messages from the Source to the Application.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal class TWCallback2
	{
		[MarshalAs(UnmanagedType.FunctionPtr)]
		private CallbackDelegate _callBackProc;

		private UIntPtr _refCon;

		private short _message;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWCallback2" /> class.
		/// </summary>
		/// <param name="callback">The callback function’s entry point.</param>
		public TWCallback2(CallbackDelegate callback)
		{
			_callBackProc = callback;
		}
	}
	/// <summary>
	/// Used by an application either to get information about, or control the setting of a capability.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public sealed class TWCapability : IDisposable
	{
		private ushort _cap;

		private ushort _conType;

		private IntPtr _hContainer;

		/// <summary>
		/// Id of capability to set or get.
		/// </summary>
		public CapabilityId Capability
		{
			get
			{
				return (CapabilityId)_cap;
			}
			set
			{
				_cap = (ushort)value;
			}
		}

		/// <summary>
		/// The type of the container structure referenced by the pointer internally. The container
		/// will be one of four types: <see cref="T:NTwain.Data.TWArray" />, <see cref="T:NTwain.Data.TWEnumeration" />,
		/// <see cref="T:NTwain.Data.TWOneValue" />, or <see cref="T:NTwain.Data.TWRange" />.
		/// </summary>
		public ContainerType ContainerType
		{
			get
			{
				return (ContainerType)_conType;
			}
			set
			{
				_conType = (ushort)value;
			}
		}

		internal IntPtr Container => _hContainer;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWCapability" /> class.
		/// </summary>
		/// <param name="capability">The capability.</param>
		public TWCapability(CapabilityId capability)
		{
			Capability = capability;
			ContainerType = ContainerType.DoNotCare;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWCapability" /> class.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <param name="value">The value.</param>
		public TWCapability(CapabilityId capability, TWOneValue value)
		{
			Capability = capability;
			SetOneValue(value, PlatformInfo.Current.MemoryManager);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWCapability" /> class.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <param name="value">The value.</param>
		/// <param name="type">The type.</param>
		public TWCapability(CapabilityId capability, string value, ItemType type)
		{
			Capability = capability;
			SetOneValue(value, type, PlatformInfo.Current.MemoryManager);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWCapability" /> class.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <param name="value">The value.</param>
		public TWCapability(CapabilityId capability, TWFrame value)
		{
			Capability = capability;
			SetOneValue(value, PlatformInfo.Current.MemoryManager);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWCapability" /> class.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <param name="value">The value.</param>
		public TWCapability(CapabilityId capability, TWEnumeration value)
		{
			Capability = capability;
			SetEnumValue(value, PlatformInfo.Current.MemoryManager);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWCapability" /> class.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <param name="value">The value.</param>
		public TWCapability(CapabilityId capability, TWRange value)
		{
			Capability = capability;
			SetRangeValue(value, PlatformInfo.Current.MemoryManager);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWCapability" /> class.
		/// </summary>
		/// <param name="capability">The capability.</param>
		/// <param name="value">The value.</param>
		public TWCapability(CapabilityId capability, TWArray value)
		{
			Capability = capability;
			SetArrayValue(value, PlatformInfo.Current.MemoryManager);
		}

		private void SetOneValue(string value, ItemType type, IMemoryManager memoryManager)
		{
			ContainerType = ContainerType.OneValue;
			if (type - 9 <= ItemType.UInt8)
			{
				_hContainer = memoryManager.Allocate((uint)(Marshal.SizeOf(typeof(TWFrame)) + 2));
				if (_hContainer != IntPtr.Zero)
				{
					IntPtr baseAddr = memoryManager.Lock(_hContainer);
					int offset = 0;
					baseAddr.WriteValue(ref offset, ItemType.UInt16, type);
					baseAddr.WriteValue(ref offset, type, value);
					memoryManager.Unlock(_hContainer);
				}
				return;
			}
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Type {0} is not valid for string.", type));
		}

		private void SetOneValue(TWFrame value, IMemoryManager memoryManager)
		{
			ContainerType = ContainerType.OneValue;
			_hContainer = memoryManager.Allocate((uint)(Marshal.SizeOf(typeof(TWFrame)) + 2));
			if (_hContainer != IntPtr.Zero)
			{
				IntPtr baseAddr = memoryManager.Lock(_hContainer);
				int offset = 0;
				baseAddr.WriteValue(ref offset, ItemType.UInt16, ItemType.Frame);
				baseAddr.WriteValue(ref offset, ItemType.Frame, value);
				memoryManager.Unlock(_hContainer);
			}
		}

		private void SetOneValue(TWOneValue value, IMemoryManager memoryManager)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			ContainerType = ContainerType.OneValue;
			if (value.ItemType.GetItemTypeSize() > 4)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.BadValueType, "TWOneValue"));
			}
			_hContainer = memoryManager.Allocate((uint)Marshal.SizeOf(value));
			if (_hContainer != IntPtr.Zero)
			{
				Marshal.StructureToPtr(value, _hContainer, fDeleteOld: false);
			}
		}

		private void SetEnumValue(TWEnumeration value, IMemoryManager memoryManager)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			ContainerType = ContainerType.Enum;
			int size = 14 + value.ItemList.Length * value.ItemType.GetItemTypeSize();
			int offset = 0;
			_hContainer = memoryManager.Allocate((uint)size);
			if (_hContainer != IntPtr.Zero)
			{
				IntPtr baseAddr = memoryManager.Lock(_hContainer);
				baseAddr.WriteValue(ref offset, ItemType.UInt16, value.ItemType);
				baseAddr.WriteValue(ref offset, ItemType.UInt32, (uint)value.ItemList.Length);
				baseAddr.WriteValue(ref offset, ItemType.UInt32, value.CurrentIndex);
				baseAddr.WriteValue(ref offset, ItemType.UInt32, value.DefaultIndex);
				object[] itemList = value.ItemList;
				foreach (object value2 in itemList)
				{
					baseAddr.WriteValue(ref offset, value.ItemType, value2);
				}
				memoryManager.Unlock(_hContainer);
			}
		}

		private void SetRangeValue(TWRange value, IMemoryManager memoryManager)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			ContainerType = ContainerType.Range;
			if (value.ItemType.GetItemTypeSize() > 4)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.BadValueType, "TWRange"));
			}
			_hContainer = memoryManager.Allocate((uint)Marshal.SizeOf(value));
			if (_hContainer != IntPtr.Zero)
			{
				Marshal.StructureToPtr(value, _hContainer, fDeleteOld: false);
			}
		}

		private void SetArrayValue(TWArray value, IMemoryManager memoryManager)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			ContainerType = ContainerType.Array;
			int size = 6 + value.ItemList.Length * value.ItemType.GetItemTypeSize();
			int offset = 0;
			_hContainer = memoryManager.Allocate((uint)size);
			if (_hContainer != IntPtr.Zero)
			{
				IntPtr baseAddr = memoryManager.Lock(_hContainer);
				baseAddr.WriteValue(ref offset, ItemType.UInt16, value.ItemType);
				baseAddr.WriteValue(ref offset, ItemType.UInt32, (uint)value.ItemList.Length);
				object[] itemList = value.ItemList;
				foreach (object value2 in itemList)
				{
					baseAddr.WriteValue(ref offset, value.ItemType, value2);
				}
				memoryManager.Unlock(_hContainer);
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (_hContainer != IntPtr.Zero)
			{
				PlatformInfo.Current.MemoryManager.Free(_hContainer);
				_hContainer = IntPtr.Zero;
			}
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="T:NTwain.Data.TWCapability" /> class.
		/// </summary>
		~TWCapability()
		{
			Dispose(disposing: false);
		}
	}
	/// <summary>
	/// Embedded in the <see cref="T:NTwain.Data.TWCieColor" /> structure;
	/// defines a CIE XYZ space tri-stimulus value.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct TWCiePoint : IEquatable<TWCiePoint>
	{
		private TWFix32 _x;

		private TWFix32 _y;

		private TWFix32 _z;

		/// <summary>
		/// First tri-stimulus value of the CIE space representation.
		/// </summary>
		public float X => _z;

		/// <summary>
		/// Second tri-stimulus value of the CIE space representation.
		/// </summary>
		public float Y => _z;

		/// <summary>
		/// Third tri-stimulus value of the CIE space representation.
		/// </summary>
		public float Z => _z;

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (!(obj is TWCiePoint))
			{
				return false;
			}
			return Equals((TWCiePoint)obj);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns></returns>
		public bool Equals(TWCiePoint other)
		{
			if (_x == other._x && _y == other._y)
			{
				return _z == other._z;
			}
			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="value1">The value1.</param>
		/// <param name="value2">The value2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(TWCiePoint value1, TWCiePoint value2)
		{
			return value1.Equals(value2);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="value1">The value1.</param>
		/// <param name="value2">The value2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(TWCiePoint value1, TWCiePoint value2)
		{
			return !value1.Equals(value2);
		}
	}
	/// <summary>
	/// Defines the mapping from an RGB color space device into CIE 1931 (XYZ) color space.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWCieColor
	{
		private ushort _colorSpace;

		private short _lowEndian;

		private short _deviceDependent;

		private int _versionNumber;

		private TWTransformStage _stageABC;

		private TWTransformStage _stageLMN;

		private TWCiePoint _whitePoint;

		private TWCiePoint _blackPoint;

		private TWCiePoint _whitePaper;

		private TWCiePoint _blackInk;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		private TWFix32[] _samples;

		/// <summary>
		/// Defines the original color space that was transformed into CIE XYZ. 
		/// This value is not set-able by the application. 
		/// </summary>
		public ushort ColorSpace => _colorSpace;

		/// <summary>
		/// Used to indicate which data byte is taken first. If zero, then high byte is
		/// first. If non-zero, then low byte is first.
		/// </summary>
		public short LowEndian => _lowEndian;

		/// <summary>
		/// If non-zero then color data is device-dependent and only ColorSpace is
		/// valid in this structure.
		/// </summary>
		public short DeviceDependent => _deviceDependent;

		/// <summary>
		/// Version of the color space descriptor specification used to define the
		/// transform data. The current version is zero.
		/// </summary>
		public int VersionNumber => _versionNumber;

		/// <summary>
		/// Describes parametrics for the first stage transformation of the Postscript
		/// Level 2 CIE color space transform process.
		/// </summary>
		public TWTransformStage StageABC => _stageABC;

		/// <summary>
		/// Describes parametrics for the first stage transformation of the Postscript
		/// Level 2 CIE color space transform process.
		/// </summary>
		public TWTransformStage StageLMN => _stageLMN;

		/// <summary>
		/// Values that specify the CIE 1931 (XYZ space) tri-stimulus value of the
		/// diffused white point.
		/// </summary>
		public TWCiePoint WhitePoint => _whitePoint;

		/// <summary>
		/// Values that specify the CIE 1931 (XYZ space) tri-stimulus value of the
		/// diffused black point.
		/// </summary>
		public TWCiePoint BlackPoint => _blackPoint;

		/// <summary>
		/// Values that specify the CIE 1931 (XYZ space) tri-stimulus value of inkless
		/// "paper" from which the image was acquired.
		/// </summary>
		public TWCiePoint WhitePaper => _whitePaper;

		/// <summary>
		/// Values that specify the CIE 1931 (XYZ space) tri-stimulus value of solid
		/// black ink on the "paper" from which the image was acquired.
		/// </summary>
		public TWCiePoint BlackInk => _blackInk;

		/// <summary>
		/// Optional table look-up values used by the decode function. Samples
		/// are ordered sequentially and end-to-end as A, B, C, L, M, and N.
		/// </summary>
		public TWFix32[] Samples => _samples;

		internal TWCieColor()
		{
		}
	}
	/// <summary>
	/// Allows for a data source and application to pass custom data to each other.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal class TWCustomDSData
	{
		private uint _infoLength;

		private IntPtr _hData;

		/// <summary>
		/// Length, in bytes, of data.
		/// </summary>
		public uint InfoLength
		{
			get
			{
				return _infoLength;
			}
			set
			{
				_infoLength = value;
			}
		}

		/// <summary>
		/// Handle to memory containing InfoLength bytes of data.
		/// </summary>
		public IntPtr hData
		{
			get
			{
				return _hData;
			}
			set
			{
				_hData = value;
			}
		}
	}
	/// <summary>
	/// Provides information about the Event that was raised by the Source. The Source should only fill
	/// in those fields applicable to the Event. The Application must only read those fields applicable to
	/// the Event.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	[BestFitMapping(false, ThrowOnUnmappableChar = true)]
	public class TWDeviceEvent
	{
		private uint _event;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		private string _deviceName;

		private uint _batteryMinutes;

		private short _batteryPercentage;

		private int _powerSupply;

		private TWFix32 _xResolution;

		private TWFix32 _yResolution;

		private uint _flashUsed2;

		private uint _automaticCapture;

		private uint _timeBeforeFirstCapture;

		private uint _timeBetweenCaptures;

		/// <summary>
		/// Defines event that has taken place.
		/// </summary>
		public DeviceEvent Event => (DeviceEvent)_event;

		/// <summary>
		/// The name of the device that generated the event.
		/// </summary>
		public string DeviceName => _deviceName;

		/// <summary>
		/// Battery minutes remaining. Valid for BatteryCheck event only.
		/// </summary>
		public int BatteryMinutes => (int)_batteryMinutes;

		/// <summary>
		/// Battery percentage remaining. Valid for BatteryCheck event only.
		/// </summary>
		public int BatteryPercentage => _batteryPercentage;

		/// <summary>
		/// Current power supply in use. Valid for PowerSupply event only.
		/// </summary>
		public int PowerSupply => _powerSupply;

		/// <summary>
		/// Current X Resolution. Valid for Resolution event only.
		/// </summary>
		public float XResolution => _xResolution;

		/// <summary>
		/// Current Y Resolution. Valid for Resolution event only.
		/// </summary>
		public float YResolution => _yResolution;

		/// <summary>
		/// Current flash setting. Valid for BatteryCheck event only.
		/// </summary>
		public FlashedUsed FlashUsed2 => (FlashedUsed)_flashUsed2;

		/// <summary>
		/// Number of images camera will capture. Valid for AutomaticCapture event only.
		/// </summary>
		public int AutomaticCapture => (int)_automaticCapture;

		/// <summary>
		/// Number of seconds before first capture. Valid for AutomaticCapture event only.
		/// </summary>
		public int TimeBeforeFirstCapture => (int)_timeBeforeFirstCapture;

		/// <summary>
		/// Hundredths of a second between captures. Valid for AutomaticCapture event only.
		/// </summary>
		public int TimeBetweenCaptures => (int)_timeBetweenCaptures;

		internal TWDeviceEvent()
		{
		}
	}
	/// <summary>
	/// Embedded in the <see cref="T:NTwain.Data.TWGrayResponse" />, <see cref="T:NTwain.Data.TWPalette8" />, and <see cref="T:NTwain.Data.TWRgbResponse" /> structures.
	/// This structure holds the tri-stimulus color palette information for <see cref="T:NTwain.Data.TWPalette8" /> structures.
	/// The order of the channels shall match their alphabetic representation. That is, for RGB data, R
	/// shall be channel 1. For CMY data, C shall be channel 1. This allows the application and Source
	/// to maintain consistency. Grayscale data will have the same values entered in all three channels.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct TWElement8 : IEquatable<TWElement8>
	{
		private byte _index;

		private byte _channel1;

		private byte _channel2;

		private byte _channel3;

		/// <summary>
		/// Value used to index into the color table.
		/// </summary>
		public byte Index
		{
			get
			{
				return _index;
			}
			set
			{
				_index = value;
			}
		}

		/// <summary>
		/// First tri-stimulus value (e.g. Red).
		/// </summary>
		public byte Channel1
		{
			get
			{
				return _channel1;
			}
			set
			{
				_channel1 = value;
			}
		}

		/// <summary>
		/// Second tri-stimulus value (e.g Green).
		/// </summary>
		public byte Channel2
		{
			get
			{
				return _channel2;
			}
			set
			{
				_channel2 = value;
			}
		}

		/// <summary>
		/// Third  tri-stimulus value (e.g Blue).
		/// </summary>
		public byte Channel3
		{
			get
			{
				return _channel3;
			}
			set
			{
				_channel3 = value;
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (!(obj is TWElement8))
			{
				return false;
			}
			return Equals((TWElement8)obj);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns></returns>
		public bool Equals(TWElement8 other)
		{
			if (_channel1 == other._channel1 && _channel2 == other._channel2 && _channel3 == other._channel3)
			{
				return _index == other._index;
			}
			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return _channel1.GetHashCode() ^ _channel2.GetHashCode() ^ _channel3.GetHashCode() ^ _index.GetHashCode();
		}

		/// <summary>
		/// Check for value equality.
		/// </summary>
		/// <param name="v1">The v1.</param>
		/// <param name="v2">The v2.</param>
		/// <returns></returns>
		public static bool operator ==(TWElement8 v1, TWElement8 v2)
		{
			return v1.Equals(v2);
		}

		/// <summary>
		/// Check for value inequality.
		/// </summary>
		/// <param name="v1">The v1.</param>
		/// <param name="v2">The v2.</param>
		/// <returns></returns>
		public static bool operator !=(TWElement8 v1, TWElement8 v2)
		{
			return !(v1 == v2);
		}
	}
	/// <summary>
	/// An enumeration stores a list of individual values, with one of the items designated as the current
	/// value. There is no required order to the values in the list.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWEnumeration
	{
		private ushort _itemType;

		private uint _numItems;

		private uint _currentIndex;

		private uint _defaultIndex;

		private object[] _itemList;

		/// <summary>
		/// Gets the byte offset of the item list from a Ptr to the first item.
		/// </summary>
		internal const int ItemOffset = 14;

		/// <summary>
		/// The type of items in the enumerated list. All items in the array have the same size.
		/// </summary>
		public ItemType ItemType
		{
			get
			{
				return (ItemType)_itemType;
			}
			set
			{
				_itemType = (ushort)value;
			}
		}

		/// <summary>
		/// The item number, or index (zero-based) into <see cref="P:NTwain.Data.TWEnumeration.ItemList" />, of the "current"
		/// value for the capability.
		/// </summary>
		public int CurrentIndex
		{
			get
			{
				return (int)_currentIndex;
			}
			set
			{
				_currentIndex = (uint)value;
			}
		}

		/// <summary>
		/// The item number, or index (zero-based) into <see cref="P:NTwain.Data.TWEnumeration.ItemList" />, of the "power-on"
		/// value for the capability.
		/// </summary>
		public int DefaultIndex
		{
			get
			{
				return (int)_defaultIndex;
			}
			set
			{
				_defaultIndex = (uint)value;
			}
		}

		/// <summary>
		/// The enumerated list: one value resides within each array element.
		/// </summary>
		public object[] ItemList
		{
			get
			{
				return _itemList;
			}
			set
			{
				_itemList = value;
				if (value != null)
				{
					_numItems = (uint)value.Length;
				}
				else
				{
					_numItems = 0u;
				}
			}
		}
	}
	/// <summary>
	/// Used on Windows and Macintosh pre OS X to pass application events/messages from the
	/// application to the Source.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWEvent
	{
		private IntPtr _pEvent;

		private ushort _tWMessage;

		/// <summary>
		/// A pointer to the event/message to be examined by the Source.
		/// Under Microsoft Windows, pEvent is a pMSG (pointer to a Microsoft
		/// Windows MSG struct). That is, the message the application received from
		/// GetMessage(). On the Macintosh, pEvent is a pointer to an EventRecord.
		/// </summary>
		public IntPtr pEvent
		{
			get
			{
				return _pEvent;
			}
			set
			{
				_pEvent = value;
			}
		}

		/// <summary>
		/// Any message the Source needs to send to the application in
		/// response to processing the event/message. The messages currently defined for
		/// this purpose are <see cref="F:NTwain.Data.Message.Null" />, <see cref="F:NTwain.Data.Message.XferReady" /> 
		/// and <see cref="F:NTwain.Data.Message.CloseDSReq" />.
		/// </summary>
		public Message TWMessage => (Message)_tWMessage;
	}
	/// <summary>
	/// This structure is used to pass specific information between the data source and the application
	/// through <see cref="T:NTwain.Data.TWExtImageInfo" />.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	[DebuggerDisplay("ID = {InfoID}, Type = {ItemType}")]
	public struct TWInfo
	{
		private ushort _infoID;

		private ushort _itemType;

		private ushort _numItems;

		private ushort _returnCode;

		private IntPtr _item;

		/// <summary>
		/// Tag identifying an information.
		/// </summary>
		public ExtendedImageInfo InfoID
		{
			get
			{
				return (ExtendedImageInfo)_infoID;
			}
			set
			{
				_infoID = (ushort)value;
			}
		}

		/// <summary>
		/// Item data type.
		/// </summary>
		public ItemType ItemType
		{
			get
			{
				return (ItemType)_itemType;
			}
			set
			{
				_itemType = (ushort)value;
			}
		}

		/// <summary>
		/// Number of items.
		/// </summary>
		public ushort NumItems => _numItems;

		/// <summary>
		/// This is the return code of availability of data for extended image attribute requested.
		/// </summary>
		/// <value>
		/// The return code.
		/// </value>
		public ReturnCode ReturnCode => (ReturnCode)_returnCode;

		/// <summary>
		/// Contains either data or a handle to data. The field
		/// contains data if the total amount of data is less than or equal to four bytes. The
		/// field contains a handle if the total amount of data is more than four bytes.
		/// The amount of data is determined by multiplying NumItems times
		/// the byte size of the data type specified by ItemType.
		/// If the Item field contains a handle to data, then the Application is
		/// responsible for freeing that memory.
		/// </summary>
		public IntPtr Item
		{
			get
			{
				return _item;
			}
			internal set
			{
				_item = value;
			}
		}

		private bool ItemIsPointer
		{
			get
			{
				if (ItemType != ItemType.Handle)
				{
					return ItemType.GetItemTypeSize() * NumItems > 4;
				}
				return true;
			}
		}

		/// <summary>
		/// Try to reads the values from the <see cref="P:NTwain.Data.TWInfo.Item" /> property.
		/// </summary>
		/// <returns></returns>
		public IList<object> ReadValues()
		{
			List<object> list = new List<object>();
			if (NumItems > 0)
			{
				if (ItemIsPointer)
				{
					if (Item != IntPtr.Zero)
					{
						IntPtr intPtr = IntPtr.Zero;
						try
						{
							int offset = 0;
							intPtr = PlatformInfo.Current.MemoryManager.Lock(Item);
							for (int i = 0; i < NumItems; i++)
							{
								list.Add(intPtr.ReadValue(ref offset, ItemType));
							}
						}
						finally
						{
							if (intPtr != IntPtr.Zero)
							{
								PlatformInfo.Current.MemoryManager.Unlock(Item);
							}
						}
					}
				}
				else
				{
					IntPtr intPtr2 = IntPtr.Zero;
					try
					{
						intPtr2 = Marshal.AllocHGlobal(IntPtr.Size);
						Marshal.WriteIntPtr(intPtr2, Item);
						int offset2 = 0;
						list.Add(intPtr2.ReadValue(ref offset2, ItemType));
					}
					finally
					{
						if (intPtr2 != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(intPtr2);
						}
					}
				}
			}
			return list;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
		}

		private void Dispose(bool disposing)
		{
			if (ItemIsPointer && Item != IntPtr.Zero)
			{
				PlatformInfo.Current.MemoryManager.Free(Item);
			}
			Item = IntPtr.Zero;
		}
	}
	/// <summary>
	/// This structure is used to pass extended image information from the data source to application at
	/// the end of State 7. The application creates this structure at the end of State 7, when it receives
	/// XFERDONE. Application fills NumInfos for Number information it needs, and array of
	/// extended information attributes in Info[ ] array. Application, then, sends it down to the source
	/// using the above operation triplet. The data source then examines each Info, and fills the rest of
	/// data with information allocating memory when necessary.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal sealed class TWExtImageInfo
	{
		private uint _numInfos;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
		private TWInfo[] _info;

		/// <summary>
		/// Number of information that application is requesting. This is filled by the
		/// application. If positive, then the application is requesting specific extended
		/// image information. The application should allocate memory and fill in the
		/// attribute tag for image information.
		/// </summary>
		public uint NumInfos
		{
			set
			{
				_numInfos = value;
			}
		}

		/// <summary>
		/// Array of information.
		/// </summary>
		public TWInfo[] Info => _info;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWExtImageInfo" /> class.
		/// </summary>
		public TWExtImageInfo()
		{
			_info = new TWInfo[200];
		}
	}
	/// <summary>
	/// Provides information about the currently selected device.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	[BestFitMapping(false, ThrowOnUnmappableChar = true)]
	public class TWFileSystem
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		private string _inputName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		private string _outputName;

		private IntPtr _context;

		private ushort _subdirectories;

		private int _fileType;

		private uint _size;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
		private string _createTimeDate;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
		private string _modifiedTimeDate;

		private uint _freeSpace;

		private int _newImageSize;

		private uint _numberOfFiles;

		private uint _numberOfSnippets;

		private uint _deviceGroupMask;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 508)]
		private sbyte[] _reserved;

		/// <summary>
		/// The name of the input or source file.
		/// </summary>
		public string InputName
		{
			get
			{
				return _inputName;
			}
			set
			{
				_inputName = value;
			}
		}

		/// <summary>
		/// The result of an operation or the name of a destination file.
		/// </summary>
		public string OutputName
		{
			get
			{
				return _outputName;
			}
			set
			{
				_outputName = value;
			}
		}

		/// <summary>
		/// A pointer to Source specific data used to remember state
		/// information, such as the current directory.
		/// </summary>
		public IntPtr Context
		{
			get
			{
				return _context;
			}
			set
			{
				_context = value;
			}
		}

		/// <summary>
		/// When set to TRUE recursively apply the operation. (ex: deletes
		/// all subdirectories in the directory being deleted; or copies all
		/// sub-directories in the directory being copied.
		/// </summary>
		public bool Recursive
		{
			get
			{
				return _subdirectories == 1;
			}
			set
			{
				_subdirectories = (value ? ((ushort)1) : ((ushort)0));
			}
		}

		/// <summary>
		/// Gets the type of the file.
		/// </summary>
		/// <value>
		/// The type of the file.
		/// </value>
		public FileType FileType
		{
			get
			{
				return (FileType)_fileType;
			}
			set
			{
				_fileType = (int)value;
			}
		}

		/// <summary>
		/// If <see cref="F:NTwain.Data.FileType.Directory" />, total size of media in bytes.
		/// If <see cref="F:NTwain.Data.FileType.Image" />, size of image in bytes.
		/// </summary>
		/// <value>
		/// The size.
		/// </value>
		public uint Size
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;
			}
		}

		/// <summary>
		/// The create date of the file, in the form "YYYY/MM/DD
		/// HH:mm:SS:sss" where YYYY is the year, MM is the numerical
		/// month, DD is the numerical day, HH is the hour, mm is the
		/// minute, SS is the second, and sss is the millisecond.
		/// </summary>
		public string CreateTimeDate
		{
			get
			{
				return _createTimeDate;
			}
			set
			{
				_createTimeDate = value;
			}
		}

		/// <summary>
		/// Last date the file was modified. Same format as <see cref="P:NTwain.Data.TWFileSystem.CreateTimeDate" />.
		/// </summary>
		public string ModifiedTimeDate
		{
			get
			{
				return _modifiedTimeDate;
			}
			set
			{
				_modifiedTimeDate = value;
			}
		}

		/// <summary>
		/// The bytes of free space left on the current device.
		/// </summary>
		public uint FreeSpace
		{
			get
			{
				return _freeSpace;
			}
			set
			{
				_freeSpace = value;
			}
		}

		/// <summary>
		/// An estimate of the amount of space a new image would take
		/// up, based on image layout, resolution and compression.
		/// Dividing this value into the FreeSpace will yield the
		/// approximate number of images that the Device has room for.
		/// </summary>
		public int NewImageSize
		{
			get
			{
				return _newImageSize;
			}
			set
			{
				_newImageSize = value;
			}
		}

		/// <summary>
		/// If applicable, return the number of <see cref="F:NTwain.Data.FileType.Image" /> files on the file system including those in all sub-directories.
		/// </summary>
		/// <value>
		/// The number of files.
		/// </value>
		public uint NumberOfFiles
		{
			get
			{
				return _numberOfFiles;
			}
			set
			{
				_numberOfFiles = value;
			}
		}

		/// <summary>
		/// The number of audio snippets associated with a file of type <see cref="F:NTwain.Data.FileType.Image" />.
		/// </summary>
		/// <value>
		/// The number of snippets.
		/// </value>
		public uint NumberOfSnippets
		{
			get
			{
				return _numberOfSnippets;
			}
			set
			{
				_numberOfSnippets = value;
			}
		}

		/// <summary>
		/// Used to group cameras (ex: front/rear bitonal, front/rear grayscale...).
		/// </summary>
		public uint DeviceGroupMask
		{
			get
			{
				return _deviceGroupMask;
			}
			set
			{
				_deviceGroupMask = value;
			}
		}
	}
	/// <summary>
	/// This structure is used by the application to specify a set of mapping values to be applied to
	/// grayscale data.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWGrayResponse
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		private TWElement8[] _response;

		/// <summary>
		/// Transfer curve descriptors. All three channels (Channel1, Channel2
		/// and Channel3) must contain the same value for every entry.
		/// </summary>
		public TWElement8[] Response
		{
			get
			{
				return _response;
			}
			set
			{
				_response = value;
			}
		}
	}
	/// <summary>
	/// A general way to describe the version of software that is running.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	[BestFitMapping(false, ThrowOnUnmappableChar = true)]
	public struct TWVersion : IEquatable<TWVersion>
	{
		private ushort _majorNum;

		private ushort _minorNum;

		private ushort _language;

		private ushort _country;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
		private string _info;

		/// <summary>
		/// This refers to your application or Source’s major revision number. e.g. The
		/// "2" in "version 2.1".
		/// </summary>
		public short Major
		{
			get
			{
				return (short)_majorNum;
			}
			set
			{
				_majorNum = (ushort)value;
			}
		}

		/// <summary>
		/// The incremental revision number of your application or Source. e.g. The "1"
		/// in "version 2.1".
		/// </summary>
		public short Minor
		{
			get
			{
				return (short)_minorNum;
			}
			set
			{
				_minorNum = (ushort)value;
			}
		}

		/// <summary>
		/// The primary language for your Source or application.
		/// </summary>
		public Language Language
		{
			get
			{
				return (Language)_language;
			}
			set
			{
				_language = (ushort)value;
			}
		}

		/// <summary>
		/// The primary country where your Source or application is intended to be
		/// distributed. e.g. Germany.
		/// </summary>
		public Country Country
		{
			get
			{
				return (Country)_country;
			}
			set
			{
				_country = (ushort)value;
			}
		}

		/// <summary>
		/// General information string - fill in as needed. e.g. "1.0b3 Beta release".
		/// </summary>
		public string Info
		{
			get
			{
				return _info;
			}
			set
			{
				_info.VerifyLengthUnder(33);
				_info = value;
			}
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1} {2}", Major, Minor, Info);
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (!(obj is TWVersion))
			{
				return false;
			}
			return Equals((TWVersion)obj);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns></returns>
		public bool Equals(TWVersion other)
		{
			if (_majorNum == other._majorNum && _minorNum == other._minorNum && _language == other._language && _country == other._country)
			{
				return string.Equals(_info, other._info);
			}
			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return _majorNum.GetHashCode() ^ _minorNum.GetHashCode() ^ _language.GetHashCode() ^ _country.GetHashCode() ^ ((_info != null) ? _info.GetHashCode() : 0);
		}

		/// <summary>
		/// Check for value equality.
		/// </summary>
		/// <param name="v1">The v1.</param>
		/// <param name="v2">The v2.</param>
		/// <returns></returns>
		public static bool operator ==(TWVersion v1, TWVersion v2)
		{
			return v1.Equals(v2);
		}

		/// <summary>
		/// Check for value inequality.
		/// </summary>
		/// <param name="v1">The v1.</param>
		/// <param name="v2">The v2.</param>
		/// <returns></returns>
		public static bool operator !=(TWVersion v1, TWVersion v2)
		{
			return !(v1 == v2);
		}
	}
	/// <summary>
	/// Provides identification information about a TWAIN entity. Used to maintain consistent
	/// communication between entities.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	[BestFitMapping(false, ThrowOnUnmappableChar = true)]
	public class TWIdentity
	{
		private uint _id;

		private TWVersion _version;

		private ushort _protocolMajor;

		private ushort _protocolMinor;

		private uint _supportedGroups;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
		private string _manufacturer;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
		private string _productFamily;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
		private string _productName;

		/// <summary>
		/// A unique, internal identifier for the TWAIN entity. This field is only
		/// filled by the Source Manager. Neither an application nor a Source
		/// should fill this field. The Source uses the contents of this field to
		/// "identify" which application is invoking the operation sent to the
		/// Source.
		/// </summary>
		public int Id => (int)_id;

		/// <summary>
		/// Gets the supported data group. The application will normally set this field to specify which Data
		/// Group(s) it wants the Source Manager to sort Sources by when
		/// presenting the Select Source dialog, or returning a list of available
		/// Sources.
		/// </summary>
		/// <value>The data group.</value>
		public DataGroups DataGroup
		{
			get
			{
				return (DataGroups)(_supportedGroups & 0xFFFF);
			}
			set
			{
				_supportedGroups = (uint)(value & DataGroups.Mask) | (0xFFFF0000u & _supportedGroups);
			}
		}

		/// <summary>
		/// Major number of latest TWAIN version that this element supports.
		/// </summary>
		public short ProtocolMajor
		{
			get
			{
				return (short)_protocolMajor;
			}
			set
			{
				_protocolMajor = (ushort)value;
			}
		}

		/// <summary>
		/// Minor number of latest TWAIN version that this element supports.
		/// </summary>
		public short ProtocolMinor
		{
			get
			{
				return (short)_protocolMinor;
			}
			set
			{
				_protocolMinor = (ushort)value;
			}
		}

		/// <summary>
		/// A <see cref="T:NTwain.Data.TWVersion" /> structure identifying the TWAIN entity.
		/// </summary>
		public TWVersion Version
		{
			get
			{
				return _version;
			}
			set
			{
				_version = value;
			}
		}

		/// <summary>
		/// Gets the data functionalities for TWAIN 2 detection.
		/// </summary>
		/// <value>The data functionalities.</value>
		public DataFunctionalities DataFunctionalities
		{
			get
			{
				return (DataFunctionalities)(_supportedGroups & 0xFFFF0000u);
			}
			set
			{
				_supportedGroups = (uint)(value & (DataFunctionalities)0xFFFF0000u) | (0xFFFF & _supportedGroups);
			}
		}

		/// <summary>
		/// String identifying the manufacturer of the application or Source. e.g. "Aldus".
		/// </summary>
		public string Manufacturer
		{
			get
			{
				return _manufacturer;
			}
			set
			{
				value.VerifyLengthUnder(33);
				_manufacturer = value;
			}
		}

		/// <summary>
		/// Tells an application that performs device-specific operations which
		/// product family the Source supports. This is useful when a new Source
		/// has been released and the application doesn't know about the
		/// particular Source but still wants to perform Custom operations with it.
		/// e.g. "ScanMan".
		/// </summary>
		public string ProductFamily
		{
			get
			{
				return _productFamily;
			}
			set
			{
				value.VerifyLengthUnder(33);
				_productFamily = value;
			}
		}

		/// <summary>
		/// A string uniquely identifying the Source. This is the string that will be
		/// displayed to the user at Source select-time. This string must uniquely
		/// identify your Source for the user, and should identify the application
		/// unambiguously for Sources that care. e.g. "ScanJet IIc".
		/// </summary>
		public string ProductName
		{
			get
			{
				return _productName;
			}
			set
			{
				value.VerifyLengthUnder(33);
				_productName = value;
			}
		}

		/// <summary>
		/// Creates a <see cref="T:NTwain.Data.TWIdentity" /> from assembly values.
		/// </summary>
		/// <param name="supportedGroups">The supported groups.</param>
		/// <param name="assembly">The assembly.</param>
		/// <returns></returns>
		/// <exception cref="T:System.ArgumentNullException">assembly</exception>
		/// <exception cref="T:System.ArgumentException"></exception>
		public static TWIdentity CreateFromAssembly(DataGroups supportedGroups, Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
			return Create(supportedGroups, assembly.GetName().Version, versionInfo.CompanyName, versionInfo.ProductName, versionInfo.ProductName, versionInfo.FileDescription);
		}

		/// <summary>
		/// Creates a <see cref="T:NTwain.Data.TWIdentity" /> from specified values.
		/// </summary>
		/// <param name="supportedGroups">The supported groups.</param>
		/// <param name="version">The version.</param>
		/// <param name="manufacturer">The manufacturer.</param>
		/// <param name="productFamily">The product family.</param>
		/// <param name="productName">Name of the product.</param>
		/// <param name="productDescription">The product description.</param>
		/// <returns></returns>
		/// <exception cref="T:System.ArgumentNullException">assembly</exception>
		/// <exception cref="T:System.ArgumentException"></exception>
		public static TWIdentity Create(DataGroups supportedGroups, Version version, string manufacturer, string productFamily, string productName, string productDescription)
		{
			if ((supportedGroups & DataGroups.Image) == 0 && (supportedGroups & DataGroups.Audio) == 0)
			{
				throw new ArgumentException(Resources.BadDataGroupsForAppId);
			}
			if (version == null)
			{
				throw new ArgumentNullException("version");
			}
			return new TWIdentity
			{
				Manufacturer = (string.IsNullOrEmpty(manufacturer) ? "UNKNOWN" : manufacturer),
				ProtocolMajor = 2,
				ProtocolMinor = 3,
				DataGroup = (DataGroups.Control | supportedGroups),
				DataFunctionalities = DataFunctionalities.App2,
				ProductFamily = (string.IsNullOrEmpty(productFamily) ? "UNKNOWN" : productFamily),
				ProductName = (string.IsNullOrEmpty(productName) ? "UNKNOWN" : productName),
				Version = new TWVersion
				{
					Major = (short)version.Major,
					Minor = (short)version.Minor,
					Country = Country.Usa,
					Language = Language.EnglishUSA,
					Info = (productDescription ?? string.Empty)
				}
			};
		}
	}
	/// <summary>
	/// Describes the "real" image data, that is, the complete image being transferred between the
	/// Source and application. The Source may transfer the data in a different format--the information
	/// may be transferred in "strips" or "tiles" in either compressed or uncompressed form.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWImageInfo
	{
		private TWFix32 _xResolution;

		private TWFix32 _yResolution;

		private int _imageWidth;

		private int _imageLength;

		private short _samplesPerPixel;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		private short[] _bitsPerSample;

		private short _bitsPerPixel;

		private ushort _planar;

		private short _pixelType;

		private ushort _compression;

		/// <summary>
		/// The number of pixels per ICapUnits in the horizontal direction. The
		/// current unit is assumed to be "inches" unless it has been otherwise
		/// negotiated between the application and Source.
		/// </summary>
		public float XResolution => _xResolution;

		/// <summary>
		/// The number of pixels per ICapUnits in the vertical direction.
		/// </summary>
		public float YResolution => _yResolution;

		/// <summary>
		/// How wide, in pixels, the entire image to be transferred is. If the Source
		/// doesn’t know, set this field to -1 (hand scanners may do this).
		/// </summary>
		public int ImageWidth => _imageWidth;

		/// <summary>
		/// How tall/long, in pixels, the image to be transferred is. If the Source
		/// doesn’t know, set this field to -1 (hand scanners may do this).
		/// </summary>
		public int ImageLength => _imageLength;

		/// <summary>
		/// The number of samples being returned. For R-G-B, this field would be
		/// set to 3. For C-M-Y-K, 4. For Grayscale or Black and White, 1.
		/// </summary>
		public short SamplesPerPixel => _samplesPerPixel;

		/// <summary>
		/// For each sample, the number of bits of information. 24-bit R-G-B will
		/// typically have 8 bits of information in each sample (8+8+8). Some 8-bit
		/// color is sampled at 3 bits Red, 3 bits Green, and 2 bits Blue. Such a
		/// scheme would put 3, 3, and 2 into the first 3 elements of this array. The
		/// supplied array allows up to 8 samples. Samples are not limited to 8
		/// bits. However, both the application and Source must simultaneously
		/// support sample sizes greater than 8 bits per color.
		/// </summary>
		public short[] BitsPerSample => _bitsPerSample;

		/// <summary>
		/// The number of bits in each image pixel (or bit depth). This value is
		/// invariant across the image. 24-bit R-G-B has BitsPerPixel = 24. 40-bit 
		/// CMYK has BitsPerPixel=40. 8-bit Grayscale has BitsPerPixel = 8. Black
		/// and White has BitsPerPixel = 1.
		/// </summary>
		public short BitsPerPixel => _bitsPerPixel;

		/// <summary>
		/// If SamplesPerPixel &gt; 1, indicates whether the samples follow one
		/// another on a pixel-by-pixel basis (R-G-B-R-G-B-R-G-B...) as is common
		/// with a one-pass scanner or all the pixels for each sample are grouped
		/// together (complete group of R, complete group of G, complete group of
		/// B) as is common with a three-pass scanner. If the pixel-by-pixel
		/// method (also known as "chunky") is used, the Source should set Planar
		/// = FALSE. If the grouped method (also called "planar") is used, the
		/// Source should set Planar = TRUE.
		/// </summary>
		public bool Planar => _planar == 1;

		/// <summary>
		/// This is the highest categorization for how the data being transferred
		/// should be interpreted by the application. This is how the application
		/// can tell if the data is Black and White, Grayscale, or Color. Currently,
		/// the only color type defined is "tri-stimulus", or color described by three
		/// characteristics. Most popular color description methods use tristimulus
		/// descriptors. For simplicity, the constant used to identify tristimulus
		/// color is called Rgb, for R-G-B color. There is no
		/// default for this value.
		/// </summary>
		public PixelType PixelType => (PixelType)_pixelType;

		/// <summary>
		/// The compression method used to process the data being transferred.
		/// Default is no compression.
		/// </summary>
		public CompressionType Compression => (CompressionType)_compression;

		internal TWImageInfo()
		{
		}
	}
	/// <summary>
	/// Involves information about the original size of the acquired image and its position on the
	/// original "page" relative to the "page’s" upper-left corner. Default measurements are in inches
	/// (units of measure can be changed by negotiating the ICapUnits capability). This information
	/// may be used by the application to relate the acquired (and perhaps processed image) to the
	/// original. Further, the application can, using this structure, set the size of the image it wants
	/// acquired.
	///
	/// Another attribute of this structure is the included frame, page, and document indexing
	/// information. Most Sources and applications, at least at first, will likely set all these fields to one.
	/// For Sources that can acquire more than one frame from a page in a single acquisition, the
	/// FrameNumber field will be handy. Sources that can acquire more than one page from a
	/// document feeder will use PageNumber and DocumentNumber. These fields will be especially
	/// useful for forms-processing applications and other applications with similar document tracking
	/// requirements.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWImageLayout
	{
		private TWFrame _frame;

		private uint _documentNumber;

		private uint _pageNumber;

		private uint _frameNumber;

		/// <summary>
		/// Frame coords within larger document.
		/// </summary>
		public TWFrame Frame
		{
			get
			{
				return _frame;
			}
			set
			{
				_frame = value;
			}
		}

		/// <summary>
		/// The document number, assigned by the Source, that the acquired data
		/// originated on. Useful for grouping pages together.
		/// Initial value is 1. Increment when a new document is placed into the
		/// document feeder (usually tell this has happened when the feeder
		/// empties). Reset when no longer acquiring from the feeder.
		/// </summary>
		public int DocumentNumber
		{
			get
			{
				return (int)_documentNumber;
			}
			set
			{
				_documentNumber = (uint)value;
			}
		}

		/// <summary>
		/// The page which the acquired data was captured from. Useful for
		/// grouping Frames together that are in some way related. 
		/// Initial value is 1. Increment for each page fed from
		/// a page feeder. Reset when a new document is placed into the feeder.
		/// </summary>
		public int PageNumber
		{
			get
			{
				return (int)_pageNumber;
			}
			set
			{
				_pageNumber = (uint)value;
			}
		}

		/// <summary>
		/// Usually a chronological index of the acquired frame. These frames
		/// are related to one another in some way; usually they were acquired
		/// from the same page. The Source assigns these values. Initial value is
		/// 1. Reset when a new page is acquired from.
		/// </summary>
		public int FrameNumber
		{
			get
			{
				return (int)_frameNumber;
			}
			set
			{
				_frameNumber = (uint)value;
			}
		}
	}
	/// <summary>
	/// Provides information for managing memory buffers. Memory for transfer buffers is allocated
	/// by the application--the Source is asked to fill these buffers. This structure keeps straight which
	/// entity is responsible for deallocation.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct TWMemory
	{
		private uint _flags;

		private uint _length;

		private IntPtr _theMem;

		/// <summary>
		/// Encodes which entity releases the buffer and how the buffer is referenced.
		/// </summary>
		public MemoryFlags Flags
		{
			get
			{
				return (MemoryFlags)_flags;
			}
			set
			{
				_flags = (uint)value;
			}
		}

		/// <summary>
		/// The size of the buffer in bytes. Should always be an even number and wordaligned.
		/// </summary>
		public uint Length
		{
			get
			{
				return _length;
			}
			set
			{
				_length = value;
			}
		}

		/// <summary>
		/// Reference to the buffer. May be a Pointer or a Handle (see Flags field to make
		/// this determination).
		/// </summary>
		public IntPtr TheMem
		{
			get
			{
				return _theMem;
			}
			set
			{
				_theMem = value;
			}
		}
	}
	/// <summary>
	/// Describes the form of the acquired data being passed from the Source to the application in memory transfer mode.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWImageMemXfer
	{
		private ushort _compression;

		private uint _bytesPerRow;

		private uint _columns;

		private uint _rows;

		private uint _xOffset;

		private uint _yOffset;

		private uint _bytesWritten;

		private TWMemory _memory;

		/// <summary>
		/// The compression method used to process the data being transferred.
		/// </summary>
		public CompressionType Compression => (CompressionType)_compression;

		/// <summary>
		/// The number of uncompressed bytes in each row of the piece of the image
		/// being described in this buffer.
		/// </summary>
		public uint BytesPerRow => _bytesPerRow;

		/// <summary>
		/// The number of uncompressed columns (in pixels) in this buffer.
		/// </summary>
		public uint Columns => _columns;

		/// <summary>
		/// The number or uncompressed rows (in pixels) in this buffer.
		/// </summary>
		public uint Rows => _rows;

		/// <summary>
		/// How far, in pixels, the left edge of the piece of the image being described
		/// by this structure is inset from the "left" side of the original image. If the
		/// Source is transferring in "strips", this value will equal zero. If the Source
		/// is transferring in "tiles", this value will often be non-zero.
		/// </summary>
		public uint XOffset => _xOffset;

		/// <summary>
		/// Same idea as XOffset, but the measure is in pixels from the "top" of the
		/// original image to the upper edge of this piece.
		/// </summary>
		public uint YOffset => _yOffset;

		/// <summary>
		/// The number of bytes written into the transfer buffer. This field must
		/// always be filled in correctly, whether compressed or uncompressed data
		/// is being transferred.
		/// </summary>
		public uint BytesWritten => _bytesWritten;

		/// <summary>
		/// A structure of type <see cref="T:NTwain.Data.TWMemory" /> describing who must dispose of the
		/// buffer, the actual size of the buffer, in bytes, and where the buffer is
		/// located in memory.
		/// </summary>
		internal TWMemory Memory
		{
			get
			{
				return _memory;
			}
			set
			{
				_memory = value;
			}
		}
	}
	/// <summary>
	/// Describes the information necessary to transfer a JPEG-compressed image during a buffered
	/// transfer. Images compressed in this fashion will be compatible with the JPEG File Interchange
	/// Format, version 1.1.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWJpegCompression
	{
		private ushort _colorSpace;

		private uint _subSampling;

		private ushort _numComponents;

		private ushort _restartFrequency;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		private ushort[] _quantMap;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		private TWMemory[] _quantTable;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		private ushort[] _huffmanMap;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		private TWMemory[] _huffmanDC;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		private TWMemory[] _huffmanAC;

		/// <summary>
		/// Defines the color space in which the
		/// compressed components are stored.
		/// </summary>
		public PixelType ColorSpace
		{
			get
			{
				return (PixelType)_colorSpace;
			}
			set
			{
				_colorSpace = (ushort)value;
			}
		}

		/// <summary>
		/// Encodes the horizontal and vertical subsampling in the form
		/// ABCDEFGH, where ABCD are the high-order four nibbles which
		/// represent the horizontal subsampling and EFGH are the low-order four
		/// nibbles which represent the vertical subsampling. Each nibble may
		/// have a value of 0, 1, 2, 3, or 4. However, max(A,B,C,D) * max(E,F,G,H)
		/// must be less than or equal to 10. Subsampling is irrelevant for single
		/// component images. Therefore, the corresponding nibbles should be set
		/// to 1. e.g. To indicate subsampling two Y for each U and V in a YUV
		/// space image, where the same subsampling occurs in both horizontal
		/// and vertical axes, this field would hold 0x21102110. For a grayscale
		/// image, this field would hold 0x10001000. A CMYK image could hold
		/// 0x11111111.
		/// </summary>
		public uint Subsampling
		{
			get
			{
				return _subSampling;
			}
			set
			{
				_subSampling = value;
			}
		}

		/// <summary>
		/// Number of color components in the image to be compressed.
		/// </summary>
		public ushort NumComponents
		{
			get
			{
				return _numComponents;
			}
			set
			{
				_numComponents = value;
			}
		}

		/// <summary>
		/// Number of MDUs (Minimum Data Units) between restart markers.
		/// Default is 0, indicating that no restart markers are used. An MDU is
		/// defined for interleaved data (i.e. R-G-B, Y-U-V, etc.) as a minimum
		/// complete set of 8x8 component blocks.
		/// </summary>
		public ushort RestartFrequency
		{
			get
			{
				return _restartFrequency;
			}
			set
			{
				_restartFrequency = value;
			}
		}

		/// <summary>
		/// Mapping of components to Quantization tables.
		/// </summary>
		public ushort[] QuantMap
		{
			get
			{
				return _quantMap;
			}
			set
			{
				_quantMap = value;
			}
		}

		/// <summary>
		/// Quantization tables.
		/// </summary>
		public TWMemory[] QuantTable
		{
			get
			{
				return _quantTable;
			}
			set
			{
				_quantTable = value;
			}
		}

		/// <summary>
		/// Mapping of components to Huffman tables. Null entries signify
		/// selection of the default tables.
		/// </summary>
		public ushort[] HuffmanMap
		{
			get
			{
				return _huffmanMap;
			}
			set
			{
				_huffmanMap = value;
			}
		}

		/// <summary>
		/// DC Huffman tables. Null entries signify selection of the default tables.
		/// </summary>
		public TWMemory[] HuffmanDC
		{
			get
			{
				return _huffmanDC;
			}
			set
			{
				_huffmanDC = value;
			}
		}

		/// <summary>
		/// AC Huffman tables. Null entries signify selection of the default tables.
		/// </summary>
		public TWMemory[] HuffmanAC
		{
			get
			{
				return _huffmanAC;
			}
			set
			{
				_huffmanAC = value;
			}
		}
	}
	/// <summary>
	/// Container for one value.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWOneValue
	{
		private ushort _itemType;

		private uint _item;

		/// <summary>
		/// The type of the item.
		/// </summary>
		public ItemType ItemType
		{
			get
			{
				return (ItemType)_itemType;
			}
			set
			{
				_itemType = (ushort)value;
			}
		}

		/// <summary>
		/// The value.
		/// </summary>
		public uint Item
		{
			get
			{
				return _item;
			}
			set
			{
				_item = value;
			}
		}
	}
	/// <summary>
	/// This structure holds the color palette information for buffered memory transfers of type
	/// ICapPixelType = Palette.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWPalette8
	{
		private ushort _numColors;

		private ushort _paletteType;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		private TWElement8[] _colors;

		/// <summary>
		/// Number of colors in the color table; maximum index into the color table
		/// should be one less than this (since color table indexes are zero-based).
		/// </summary>
		public ushort NumColors
		{
			get
			{
				return _numColors;
			}
			set
			{
				_numColors = value;
			}
		}

		/// <summary>
		/// Specifies type of palette.
		/// </summary>
		public PaletteType PaletteType
		{
			get
			{
				return (PaletteType)_paletteType;
			}
			set
			{
				_paletteType = (ushort)value;
			}
		}

		/// <summary>
		/// Array of palette values.
		/// </summary>
		public TWElement8[] Colors
		{
			get
			{
				return _colors;
			}
			set
			{
				_colors = value;
			}
		}
	}
	/// <summary>
	/// Used to bypass the TWAIN protocol when communicating with a device. All memory must be
	/// allocated and freed by the Application.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWPassThru
	{
		private IntPtr _pCommand;

		private uint _commandBytes;

		private int _direction;

		private IntPtr _pData;

		private uint _dataBytes;

		private uint _dataBytesXfered;

		/// <summary>
		/// Pointer to Command buffer.
		/// </summary>
		public IntPtr pCommand
		{
			get
			{
				return _pCommand;
			}
			set
			{
				_pCommand = value;
			}
		}

		/// <summary>
		/// Number of bytes in Command buffer.
		/// </summary>
		public uint CommandBytes
		{
			get
			{
				return _commandBytes;
			}
			set
			{
				_commandBytes = value;
			}
		}

		/// <summary>
		/// Defines the direction of data flow.
		/// </summary>
		public Direction Direction
		{
			get
			{
				return (Direction)_direction;
			}
			set
			{
				_direction = (int)value;
			}
		}

		/// <summary>
		/// Pointer to Data buffer.
		/// </summary>
		public IntPtr pData
		{
			get
			{
				return _pData;
			}
			set
			{
				_pData = value;
			}
		}

		/// <summary>
		/// Number of bytes in Data buffer.
		/// </summary>
		public uint DataBytes
		{
			get
			{
				return _dataBytes;
			}
			set
			{
				_dataBytes = value;
			}
		}

		/// <summary>
		/// Number of bytes successfully transferred.
		/// </summary>
		public uint DataBytesXfered
		{
			get
			{
				return _dataBytesXfered;
			}
			set
			{
				_dataBytesXfered = value;
			}
		}
	}
	/// <summary>
	/// This structure tells the application how many more complete transfers the Source currently has
	/// available.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal class TWPendingXfers
	{
		private ushort _count;

		private uint _eOJ;

		/// <summary>
		/// The number of complete transfers a Source has available for the application it is
		/// connected to. If no more transfers are available, set to zero. If an unknown and
		/// non-zero number of transfers are available, set to -1.
		/// </summary>
		public int Count
		{
			get
			{
				if (_count != ushort.MaxValue)
				{
					return _count;
				}
				return -1;
			}
		}

		/// <summary>
		/// The application should check this field if the CapJobControl is set to other
		/// than None. If this is not 0, the application should expect more data
		/// from the driver according to CapJobControl settings.
		/// </summary>
		public EndXferJob EndOfJob => (EndXferJob)_eOJ;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWPendingXfers" /> class.
		/// </summary>
		public TWPendingXfers()
		{
			_count = ushort.MaxValue;
		}
	}
	/// <summary>
	/// Container for a range of values.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWRange
	{
		private ushort _itemType;

		private uint _minValue;

		private uint _maxValue;

		private uint _stepSize;

		private uint _defaultValue;

		private uint _currentValue;

		/// <summary>
		/// The type of items in the list.
		/// </summary>
		public ItemType ItemType
		{
			get
			{
				return (ItemType)_itemType;
			}
			set
			{
				_itemType = (ushort)value;
			}
		}

		/// <summary>
		/// The least positive/most negative value of the range.
		/// </summary>
		public uint MinValue
		{
			get
			{
				return _minValue;
			}
			set
			{
				_minValue = value;
			}
		}

		/// <summary>
		/// The most positive/least negative value of the range.
		/// </summary>
		public uint MaxValue
		{
			get
			{
				return _maxValue;
			}
			set
			{
				_maxValue = value;
			}
		}

		/// <summary>
		/// The delta between two adjacent values of the range.
		/// e.g. Item2 - Item1 = StepSize;
		/// </summary>
		public uint StepSize
		{
			get
			{
				return _stepSize;
			}
			set
			{
				_stepSize = value;
			}
		}

		/// <summary>
		/// The device’s "power-on" value for the capability. If the application is
		/// performing a MSG_SET operation and isn’t sure what the default
		/// value is, set this field to <see cref="F:NTwain.Data.TwainConst.DoNotCare32" />.
		/// </summary>
		public uint DefaultValue
		{
			get
			{
				return _defaultValue;
			}
			set
			{
				_defaultValue = value;
			}
		}

		/// <summary>
		/// The value to which the device (or its user interface) is currently set to
		/// for the capability.
		/// </summary>
		public uint CurrentValue
		{
			get
			{
				return _currentValue;
			}
			set
			{
				_currentValue = value;
			}
		}
	}
	/// <summary>
	/// This structure is used by the application to specify a set of mapping values to be applied to RGB
	/// color data. Use this structure for RGB data whose bit depth is up to, and including, 8-bits.
	/// The number of elements in the array is determined by <see cref="P:NTwain.Data.TWImageInfo.BitsPerPixel" />—the number of
	/// elements is 2 raised to the power of <see cref="P:NTwain.Data.TWImageInfo.BitsPerPixel" />.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWRgbResponse
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		private TWElement8[] _response;

		/// <summary>
		/// Transfer curve descriptors. To minimize color shift problems, writing the
		/// same values into each channel is desirable.
		/// </summary>
		public TWElement8[] Response
		{
			get
			{
				return _response;
			}
			set
			{
				_response = value;
			}
		}
	}
	/// <summary>
	/// Describes the file format and file specification information for a transfer through a disk file.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	[BestFitMapping(false, ThrowOnUnmappableChar = true)]
	public class TWSetupFileXfer
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		private string _fileName;

		private ushort _format;

		private short _vRefNum = -1;

		/// <summary>
		/// A complete file specifier to the target file. On Windows, be sure to include the
		/// complete pathname.
		/// </summary>
		public string FileName
		{
			get
			{
				return _fileName;
			}
			set
			{
				value.VerifyLengthUnder(255);
				_fileName = value;
			}
		}

		/// <summary>
		/// The format of the file the Source is to fill. 
		/// </summary>
		public FileFormat Format
		{
			get
			{
				return (FileFormat)_format;
			}
			set
			{
				_format = (ushort)value;
			}
		}

		/// <summary>
		/// The volume reference number for the file. This applies to Macintosh only. On
		/// Windows, fill the field with -1.
		/// </summary>
		public short VRefNum
		{
			get
			{
				return _vRefNum;
			}
			set
			{
				_vRefNum = value;
			}
		}
	}
	/// <summary>
	/// Provides the application information about the Source’s requirements and preferences
	/// regarding allocation of transfer buffer(s).
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWSetupMemXfer
	{
		private uint _minBufSize;

		private uint _maxBufSize;

		private uint _preferred;

		/// <summary>
		/// The size of the smallest transfer buffer, in bytes, that a Source can be
		/// successful with. This will typically be the number of bytes in an
		/// uncompressed row in the block to be transferred. An application should
		/// never allocate a buffer smaller than this.
		/// </summary>
		public uint MinBufferSize => _minBufSize;

		/// <summary>
		/// The size of the largest transfer buffer, in bytes, that a Source can fill. If a
		/// Source can fill an arbitrarily large buffer, it might set this field to negative 1 to
		/// indicate this (a hand-held scanner might do this, depending on how long its
		/// cord is). Other Sources, such as frame grabbers, cannot fill a buffer larger than
		/// a certain size. Allocation of a transfer buffer larger than this value is wasteful.
		/// </summary>
		public uint MaxBufferSize => _maxBufSize;

		/// <summary>
		/// The size of the optimum transfer buffer, in bytes. A smart application will
		/// allocate transfer buffers of this size, if possible. Buffers of this size will
		/// optimize the Source’s performance. Sources should be careful to put
		/// reasonable values in this field. Buffers that are 10’s of kbytes will be easier for
		/// applications to allocate than buffers that are 100’s or 1000’s of kbytes.
		/// </summary>
		public uint Preferred => _preferred;

		internal TWSetupMemXfer()
		{
		}
	}
	/// <summary>
	/// Describes the status of a source.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWStatus
	{
		internal ushort _conditionCode;

		internal ushort _data;

		/// <summary>
		/// Condition Code describing the status.
		/// </summary>
		public ConditionCode ConditionCode => (ConditionCode)_conditionCode;

		/// <summary>
		/// Valid for TWAIN 2.1 and later. This field contains additional
		/// scanner-specific data. If there is no data, then this value must be zero.
		/// </summary>
		public ushort Data => _data;

		internal TWStatus()
		{
		}

		internal TWStatus(ushort code, ushort data)
		{
			_conditionCode = code;
			_data = data;
		}
	}
	/// <summary>
	/// Translates the contents of Status into a localized UTF8string, with the total number of bytes
	/// in the string.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public sealed class TWStatusUtf8 : IDisposable
	{
		private ushort _conditionCode;

		private ushort _data;

		private uint _size;

		private IntPtr _uTF8string;

		/// <summary>
		/// <see cref="T:NTwain.Data.TWStatus" /> data received from a previous call.
		/// </summary>
		public TWStatus Status
		{
			get
			{
				return new TWStatus(_conditionCode, _data);
			}
			internal set
			{
				_conditionCode = value._conditionCode;
				_data = value._data;
			}
		}

		/// <summary>
		/// Total number of bytes in the UTF8string, plus the terminating NULL byte. 
		/// This is not the same as the total number of characters in the string.
		/// </summary>
		public int Size => (int)_size;

		/// <summary>
		/// TW_HANDLE to a UTF-8 encoded localized string (based on 
		/// TwIdentity.Language or CapLanguage). The Source allocates
		/// it, the Application frees it.
		/// </summary>
		public IntPtr UTF8StringPtr => _uTF8string;

		/// <summary>
		/// Gets the actual string from the pointer. This may be incorrect.
		/// </summary>
		/// <returns></returns>
		public string TryGetString()
		{
			if (_uTF8string != IntPtr.Zero)
			{
				StringBuilder stringBuilder = new StringBuilder(Size - 1);
				byte value;
				while (stringBuilder.Length < _size && (value = Marshal.ReadByte(_uTF8string, stringBuilder.Length)) != 0)
				{
					stringBuilder.Append((char)value);
				}
				return stringBuilder.ToString();
			}
			return null;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (_uTF8string != IntPtr.Zero)
			{
				PlatformInfo.Current.MemoryManager.Free(_uTF8string);
				_uTF8string = IntPtr.Zero;
			}
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="T:NTwain.Data.TWStatusUtf8" /> class.
		/// </summary>
		~TWStatusUtf8()
		{
			Dispose(disposing: false);
		}
	}
	/// <summary>
	/// This structure is used to handle the user interface coordination between an application and a
	/// Source.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal class TWUserInterface
	{
		private ushort _showUI;

		private ushort _modalUI;

		private IntPtr _hParent;

		/// <summary>
		/// Set to TRUE by the application if the Source should activate its built-in user
		/// interface. Otherwise, set to FALSE. Note that not all sources support ShowUI =
		/// FALSE.
		/// </summary>
		public bool ShowUI
		{
			get
			{
				return _showUI > 0;
			}
			set
			{
				_showUI = (value ? ((ushort)1) : ((ushort)0));
			}
		}

		/// <summary>
		/// If ShowUI is TRUE, then an application setting this to TRUE requests the Source to
		/// run Modal.
		/// </summary>
		public bool ModalUI
		{
			set
			{
				_modalUI = (value ? ((ushort)1) : ((ushort)0));
			}
		}

		/// <summary>
		/// Microsoft Windows only: Application’s window handle. The Source designates
		/// the hWnd as its parent when creating the Source dialog.
		/// </summary>
		public IntPtr hParent
		{
			set
			{
				_hParent = value;
			}
		}
	}
	internal delegate ReturnCode CallbackDelegate(TWIdentity origin, TWIdentity destination, DataGroups dg, DataArgumentType dat, Message msg, IntPtr data);
	/// <summary>
	/// Provides entry points required by TWAIN 2.0 Applications and Sources.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal class TWEntryPoint : IMemoryManager
	{
		public delegate IntPtr MemAllocateDelegate(uint size);

		public delegate void MemFreeDelegate(IntPtr handle);

		public delegate IntPtr MemLockDelegate(IntPtr handle);

		public delegate void MemUnlockDelegate(IntPtr handle);

		private uint _size;

		private IntPtr _dSM_Entry;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		private MemAllocateDelegate _dSM_MemAllocate;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		private MemFreeDelegate _dSM_MemFree;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		private MemLockDelegate _dSM_MemLock;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		private MemUnlockDelegate _dSM_MemUnlock;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWEntryPoint" /> class.
		/// </summary>
		public TWEntryPoint()
		{
			_size = (uint)Marshal.SizeOf(this);
		}

		public IntPtr Allocate(uint size)
		{
			return _dSM_MemAllocate(size);
		}

		public void Free(IntPtr handle)
		{
			_dSM_MemFree(handle);
		}

		public IntPtr Lock(IntPtr handle)
		{
			return _dSM_MemLock(handle);
		}

		public void Unlock(IntPtr handle)
		{
			_dSM_MemUnlock(handle);
		}
	}
	/// <summary>
	/// The range of colors specified by this structure is replaced with Replacement grayscale value in the
	/// binary image. The color is specified in HSV color space.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWFilterDescriptor
	{
		private uint _size;

		private uint _hueStart;

		private uint _hueEnd;

		private uint _saturationStart;

		private uint _saturationEnd;

		private uint _valueStart;

		private uint _valueEnd;

		private uint _replacement;

		/// <summary>
		/// Size of this structure in bytes.
		/// </summary>
		/// <value>
		/// The size.
		/// </value>
		public uint Size
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;
			}
		}

		/// <summary>
		/// Hue starting number. Valid values 0 to 3600 (0° to 360°).
		/// </summary>
		public uint HueStart
		{
			get
			{
				return _hueStart;
			}
			set
			{
				_hueStart = value;
			}
		}

		/// <summary>
		/// Hue ending number. Valid values 0 to 3600 (0° to 360°).
		/// </summary>
		public uint HueEnd
		{
			get
			{
				return _hueEnd;
			}
			set
			{
				_hueEnd = value;
			}
		}

		/// <summary>
		/// Saturation starting number. Valid values 0 to 1000 (0% to 100%).
		/// </summary>
		public uint SaturationStart
		{
			get
			{
				return _saturationStart;
			}
			set
			{
				_saturationStart = value;
			}
		}

		/// <summary>
		/// Saturation ending number. Valid values 0 to 1000 (0% to 100%).
		/// </summary>
		public uint SaturationEnd
		{
			get
			{
				return _saturationEnd;
			}
			set
			{
				_saturationEnd = value;
			}
		}

		/// <summary>
		/// Luminosity starting number. Valid values 0 to 1000 (0% to 100%).
		/// </summary>
		public uint ValueStart
		{
			get
			{
				return _valueStart;
			}
			set
			{
				_valueStart = value;
			}
		}

		/// <summary>
		/// Luminosity ending number. Valid values 0 to 1000 (0% to 100%).
		/// </summary>
		public uint ValueEnd
		{
			get
			{
				return _valueEnd;
			}
			set
			{
				_valueEnd = value;
			}
		}

		/// <summary>
		/// Replacement grayscale value. Valid values 0 to (2^32)–1 (Maximum value
		/// depends on grayscale bit depth).
		/// </summary>
		public uint Replacement
		{
			get
			{
				return _replacement;
			}
			set
			{
				_replacement = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWFilterDescriptor" /> struct.
		/// </summary>
		public TWFilterDescriptor()
		{
			_size = (uint)Marshal.SizeOf(this);
		}
	}
	/// <summary>
	/// Specifies the filter to be applied during image acquisition. More than one descriptor can be
	/// specified. All descriptors are applied with an OR statement.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class TWFilter
	{
		private uint _size;

		private uint _descriptorCount;

		private uint _maxDescriptorCount;

		private uint _condition;

		private IntPtr _hDescriptors;

		/// <summary>
		/// Number of descriptors in hDescriptors array.
		/// </summary>
		/// <value>
		/// The descriptor count.
		/// </value>
		public uint DescriptorCount
		{
			get
			{
				return _descriptorCount;
			}
			set
			{
				_descriptorCount = value;
			}
		}

		/// <summary>
		/// Maximum possible descriptors. Valid only for GET and GETDEFAULT operations.
		/// </summary>
		/// <value>
		/// The maximum descriptor count.
		/// </value>
		public uint MaxDescriptorCount
		{
			get
			{
				return _maxDescriptorCount;
			}
			set
			{
				_maxDescriptorCount = value;
			}
		}

		/// <summary>
		/// If the value is 0 filter will check if current pixel color is inside the area
		/// specified by the descriptor. If the value is 1 it will check if it is outside
		/// of this area.
		/// </summary>
		/// <value>
		/// The condition.
		/// </value>
		public uint Condition
		{
			get
			{
				return _condition;
			}
			set
			{
				_condition = value;
			}
		}

		/// <summary>
		/// Handle to array of <see cref="T:NTwain.Data.TWFilterDescriptor" />.
		/// </summary>
		/// <value>
		/// The descriptors.
		/// </value>
		public IntPtr hDescriptors
		{
			get
			{
				return _hDescriptors;
			}
			set
			{
				_hDescriptors = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NTwain.Data.TWFilter" /> class.
		/// </summary>
		public TWFilter()
		{
			_size = (uint)Marshal.SizeOf(this);
		}
	}
	/// <summary>
	/// Indicates the type of container used in capability.
	/// Corresponds to TWON_* values.
	/// </summary>
	public enum ContainerType : ushort
	{
		/// <summary>
		/// The default value for this enum.
		/// </summary>
		Invalid = 0,
		/// <summary>
		/// The container is <see cref="T:NTwain.Data.TWArray" />.
		/// </summary>
		Array = 3,
		/// <summary>
		/// The container is <see cref="T:NTwain.Data.TWEnumeration" />.
		/// </summary>
		Enum = 4,
		/// <summary>
		/// The container is <see cref="T:NTwain.Data.TWOneValue" />.
		/// </summary>
		OneValue = 5,
		/// <summary>
		/// The container is <see cref="T:NTwain.Data.TWRange" />.
		/// </summary>
		Range = 6,
		/// <summary>
		/// The don't care value.
		/// </summary>
		DoNotCare = ushort.MaxValue
	}
	/// <summary>
	/// Flags used in <see cref="T:NTwain.Data.TWMemory" />.
	/// Corresponds to TWMF_* values.
	/// </summary>
	[Flags]
	public enum MemoryFlags : uint
	{
		None = 0u,
		AppOwns = 1u,
		DsmOwns = 2u,
		DSOwns = 4u,
		Pointer = 8u,
		Handle = 0x10u
	}
	/// <summary>
	/// The data types of item in TWAIN, used in the
	/// capability containers.
	/// Corresponds to TWTY_* values.
	/// </summary>
	public enum ItemType : ushort
	{
		/// <summary>
		/// Means Item is a an 8 bit value.
		/// </summary>
		Int8 = 0,
		/// <summary>
		/// Means Item is a 16 bit value.
		/// </summary>
		Int16 = 1,
		/// <summary>
		/// Means Item is a 32 bit value.
		/// </summary>
		Int32 = 2,
		/// <summary>
		/// Means Item is an unsigned 8 bit value.
		/// </summary>
		UInt8 = 3,
		/// <summary>
		/// Means Item is an unsigned 16 bit value.
		/// </summary>
		UInt16 = 4,
		/// <summary>
		/// Means Item is an unsigned 32 bit value.
		/// </summary>
		UInt32 = 5,
		/// <summary>
		/// Means Item is an unsigned 16 bit value (supposedly, YMMV).
		/// </summary>
		Bool = 6,
		/// <summary>
		/// Means Item is a <see cref="F:NTwain.Data.ItemType.Fix32" />.
		/// </summary>
		Fix32 = 7,
		/// <summary>
		/// Means Item is a <see cref="F:NTwain.Data.ItemType.Frame" />.
		/// </summary>
		Frame = 8,
		/// <summary>
		/// Means Item is a 32 char string (max).
		/// </summary>
		String32 = 9,
		/// <summary>
		/// Means Item is a 64 char string (max).
		/// </summary>
		String64 = 10,
		/// <summary>
		/// Means Item is a 128 char string (max).
		/// </summary>
		String128 = 11,
		/// <summary>
		/// Means Item is a char string shorter than 255.
		/// </summary>
		String255 = 12,
		/// <summary>
		/// Means Item is a handle (pointer).
		/// </summary>
		Handle = 15
	}
	/// <summary>
	/// CapAlarms values.
	/// Corresponds to TWAL_* values.
	/// </summary>
	public enum AlarmType : ushort
	{
		Alarm,
		FeederError,
		FeederWarning,
		Barcode,
		DoubleFeed,
		Jam,
		PatchCode,
		Power,
		Skew
	}
	/// <summary>
	/// ICapAutoSize values.
	/// Corresponds to TWAS_* values.
	/// </summary>
	public enum AutoSize : ushort
	{
		None,
		Auto,
		Current
	}
	/// <summary>
	/// The bar code’s orientation on the scanned image is described in
	/// reference to a Western-style interpretation of the image.
	/// Corresponds to TWBCOR_* values.
	/// </summary>
	public enum BarcodeRotation : uint
	{
		/// <summary>
		/// Normal reading orientation.
		/// </summary>
		Rot0,
		/// <summary>
		/// Rotated 90 degrees clockwise.
		/// </summary>
		Rot90,
		/// <summary>
		/// Rotated 180 degrees clockwise.
		/// </summary>
		Rot180,
		/// <summary>
		/// Rotated 270 degrees clockwise.
		/// </summary>
		Rot270,
		/// <summary>
		/// The orientation is not known.
		/// </summary>
		RotX
	}
	/// <summary>
	/// ICapBarcodeSearchMode values,
	/// ICapPatchCodeSearchMode values.
	/// Corresponds to TWBD_* values.
	/// </summary>
	public enum BarcodeDirection : ushort
	{
		Horz,
		Vert,
		HorzVert,
		VertHorz
	}
	/// <summary>
	/// ICapBitOrder/ICapBitOrderCodes values.
	/// Corresponds to TWBO_* values.
	/// </summary>
	public enum BitOrder : ushort
	{
		LsbFirst,
		MsbFirst
	}
	/// <summary>
	/// ICapAutoDiscardBlankPages values.
	/// Corresponds to TWBP_* values.
	/// </summary>
	public enum BlankPage
	{
		Invalid = 0,
		Disable = -2,
		Auto = -1
	}
	/// <summary>
	/// Values for ICapBitDepthReduction.
	/// Corresponds to TWBR_* values.
	/// </summary>
	public enum BitDepthReduction : ushort
	{
		Threshold,
		Halftone,
		CustHalftone,
		Diffusion,
		DynamicThreashold
	}
	/// <summary>
	/// ICapBarcodeSearchPriorities values,
	/// ICapSupportedBarcodeTypes values.
	/// Corresponds to TWBT_* values.
	/// </summary>
	public enum BarcodeType : ushort
	{
		ThreeOfNine,
		TwoOfFiveInterleaved,
		TwoOfFiveNonInterleaved,
		Code92,
		Code128,
		Ucc128,
		Codabar,
		UpcA,
		UpcE,
		Ean8,
		Ean13,
		PostNet,
		Pdf417,
		TwoOfFiveIndustrial,
		TwoOfFiveMatrix,
		TwoOfFiveDataLogic,
		TwoOfFiveIata,
		ThreeOfNineFullAscii,
		CodabarWithStartStop,
		MaxiCode,
		QRCode
	}
	/// <summary>
	/// ICapCompression values. Allows the application and Source to identify which compression schemes they have in common
	/// for Buffered Memory and File transfers. Since only certain file formats support compression, this capability must be negotiated after
	/// setting the desired file format with ICAP_IMAGEFILEFORMAT.
	/// Corresponds to TWCP_* values.
	/// </summary>
	public enum CompressionType : ushort
	{
		/// <summary>
		/// All Sources must support this.
		/// </summary>
		None,
		/// <summary>
		/// Can be used with TIFF or PICT
		/// </summary>
		PackBits,
		/// <summary>
		/// From the CCITT specification (now ITU), intended for document images (can be used with TIFF).
		/// </summary>
		Group31D,
		/// <summary>
		/// From the CCITT specification (now ITU), intended for document images (can be used with TIFF).
		/// </summary>
		Group31DEol,
		/// <summary>
		/// From the CCITT specification (now ITU), intended for document images (can be used with TIFF).
		/// </summary>
		Group32D,
		/// <summary>
		/// From the CCITT specification (now ITU), intended for document images (can be used with TIFF).
		/// </summary>
		Group4,
		/// <summary>
		/// Intended for the compression of color photographs (can be used with TIFF, JFIF or SPIFF).
		/// </summary>
		Jpeg,
		/// <summary>
		/// A compression licensed by UNISYS (can be used with TIFF).
		/// </summary>
		Lzw,
		/// <summary>
		/// Intended for bitonal and grayscale document images (can be used with TIFF or SPIFF).
		/// </summary>
		Jbig,
		/// <summary>
		/// This compression can only be used if ICAP_IMAGEFILEFORMAT is set to TWFF_PNG.
		/// </summary>
		Png,
		/// <summary>
		/// Can only be used if ICAP_IMAGEFILEFORMAT is set to TWFF_BMP.
		/// </summary>
		Rle4,
		/// <summary>
		/// Can only be used if ICAP_IMAGEFILEFORMAT is set to TWFF_BMP.
		/// </summary>
		Rle8,
		/// <summary>
		/// Can only be used if ICAP_IMAGEFILEFORMAT is set to TWFF_BMP.
		/// </summary>
		BitFields,
		/// <summary>
		/// Per RFC 1951 (AKA 'Flate' and 'Deflate')
		/// </summary>
		Zip,
		/// <summary>
		/// Per ISO/IEC 15444
		/// </summary>
		Jpeg2000
	}
	/// <summary>
	/// CapCameraSide/TWEI_PAGESIDE values.
	/// Corresponds to TWCS_* values.
	/// </summary>
	public enum CameraSide : ushort
	{
		Both,
		Top,
		Bottom
	}
	/// <summary>
	/// CapClearBuffers values.
	/// Corresponds to TWCB_* values.
	/// </summary>
	public enum ClearBuffer : ushort
	{
		Auto,
		Clear,
		NoClear
	}
	/// <summary>
	/// Indicates the type of event from the source.
	/// Also CapDeviceEvent values. If used as
	/// a cap value it's ushort.
	/// Corresponds to TWDE_* values.
	/// </summary>
	public enum DeviceEvent : uint
	{
		CheckAutomaticCapture = 0u,
		CheckBattery = 1u,
		CheckDeviceOnline = 2u,
		CheckFlash = 3u,
		CheckPowerSupply = 4u,
		CheckResolution = 5u,
		DeviceAdded = 6u,
		DeviceOffline = 7u,
		DeviceReady = 8u,
		DeviceRemoved = 9u,
		ImageCaptured = 10u,
		ImageDeleted = 11u,
		PaperDoubleFeed = 12u,
		PaperJam = 13u,
		LampFailure = 14u,
		PowerSave = 15u,
		PowerSaveNotify = 16u,
		CustomEvents = 32768u
	}
	/// <summary>
	/// <see cref="P:NTwain.Data.TWPassThru.Direction" /> values.
	/// Corresponds to TWDR_* values.
	/// </summary>
	public enum Direction
	{
		Invalid,
		Get,
		Set
	}
	/// <summary>
	/// TWEI_DESKEWSTATUS values.
	/// Corresponds to TWDSK_* values.
	/// </summary>
	public enum DeskewStatus : uint
	{
		Success,
		ReportOnly,
		Fail,
		Disabled
	}
	/// <summary>
	/// CapDuplex values.
	/// Corresponds to TWDX_* values.
	/// </summary>
	public enum Duplex : ushort
	{
		None,
		OnePass,
		TwoPass
	}
	/// <summary>
	/// CapFeederAlignment values.
	/// Corresponds to TWFA_* values.
	/// </summary>
	public enum FeederAlignment : ushort
	{
		None,
		Left,
		Center,
		Right
	}
	/// <summary>
	/// ICapFeederType values.
	/// Corresponds to TWFE_* values.
	/// </summary>
	public enum FeederType : ushort
	{
		General,
		Photo
	}
	/// <summary>
	/// ICapImageFileFormat values.
	/// Corresponds to TWFF_* values.
	/// </summary>
	public enum FileFormat : ushort
	{
		/// <summary>
		/// Used for document imaging. Native Linux format.
		/// </summary>
		Tiff = 0,
		/// <summary>
		/// Native Macintosh format
		/// </summary>
		Pict = 1,
		/// <summary>
		/// Native Microsoft format
		/// </summary>
		Bmp = 2,
		/// <summary>
		/// Used for document imaging
		/// </summary>
		Xbm = 3,
		/// <summary>
		/// Wrapper for JPEG images
		/// </summary>
		Jfif = 4,
		/// <summary>
		/// FlashPix, used with digital cameras
		/// </summary>
		Fpx = 5,
		/// <summary>
		/// Multi-page TIFF files
		/// </summary>
		TiffMulti = 6,
		/// <summary>
		/// An image format standard intended for use on the web, replaces GIF
		/// </summary>
		Png = 7,
		/// <summary>
		/// A standard from JPEG, intended to replace JFIF, also supports JBIG
		/// </summary>
		Spiff = 8,
		/// <summary>
		/// File format for use with digital cameras.
		/// </summary>
		Exif = 9,
		/// <summary>
		/// A file format from Adobe
		/// </summary>
		Pdf = 10,
		/// <summary>
		/// A file format from the Joint Photographic Experts Group ISO/IEC 15444-1
		/// </summary>
		Jp2 = 11,
		/// <summary>
		/// A file format from the Joint Photographic Experts Group ISO/IEC 15444-2
		/// </summary>
		Jpx = 13,
		/// <summary>
		/// A file format from LizardTech
		/// </summary>
		Dejavu = 14,
		/// <summary>
		/// A file format from Adobe PDF/A, Version 1
		/// </summary>
		PdfA = 15,
		/// <summary>
		/// A file format from Adobe PDF/A, Version 2
		/// </summary>
		PdfA2 = 16
	}
	/// <summary>
	/// ICapFlashUsed2 values.
	/// Corresponds to TWFL_* values.
	/// </summary>
	public enum FlashedUsed : ushort
	{
		None,
		Off,
		On,
		Auto,
		Redeye
	}
	/// <summary>
	/// CapFeederOrder values.
	/// Corresponds to TWFO_* values.
	/// </summary>
	public enum FeederOrder : ushort
	{
		FirstPageFirst,
		LastPageFirst
	}
	/// <summary>
	/// CapFeederPocket values.
	/// Corresponds to TWFP_* values.
	/// </summary>
	public enum FeederPocket : ushort
	{
		PocketError,
		Pocket1,
		Pocket2,
		Pocket3,
		Pocket4,
		Pocket5,
		Pocket6,
		Pocket7,
		Pocket8,
		Pocket9,
		Pocket10,
		Pocket11,
		Pocket12,
		Pocket13,
		Pocket14,
		Pocket15,
		Pocket16
	}
	/// <summary>
	/// ICapFlipRotation values.
	/// Corresponds to TWFR_* values.
	/// </summary>
	public enum FlipRotation : ushort
	{
		Book,
		Fanfold
	}
	/// <summary>
	/// ICapFilter values.
	/// Corresponds to TWFT_* values.
	/// </summary>
	public enum FilterType : ushort
	{
		Red,
		Green,
		Blue,
		None,
		White,
		Cyan,
		Magenta,
		Yellow,
		Black
	}
	/// <summary>
	/// <see cref="P:NTwain.Data.TWFileSystem.FileType" /> values.
	/// Corresponds to TWFY_* values.
	/// </summary>
	public enum FileType
	{
		Camera,
		CameraTop,
		CameraBottom,
		CameraPreview,
		Domain,
		Host,
		Directory,
		Image,
		Unknown
	}
	/// <summary>
	/// ICapIccProfile values.
	/// Corresponds to TWIC* values.
	/// </summary>
	public enum IccProfile : ushort
	{
		None,
		Link,
		Embed
	}
	/// <summary>
	/// ICapImageFilter values.
	/// Corresponds to TWIF_* values.
	/// </summary>
	public enum ImageFilter : ushort
	{
		None = 0,
		Auto = 1,
		LowPass = 2,
		BandPass = 3,
		HighPass = 4,
		Text = BandPass,
		FineLine = HighPass
	}
	/// <summary>
	/// ICapImageMerge values.
	/// Corresponds to TWIM_* values.
	/// </summary>
	public enum ImageMerge : ushort
	{
		None,
		FrontOnTop,
		FrontOnBottom,
		FrontOnLeft,
		FrontOnRight
	}
	/// <summary>
	/// CapJobControl values.
	/// Corresponds to TWJC_* values.
	/// </summary>
	public enum JobControl : ushort
	{
		/// <summary>
		/// No job control.
		/// </summary>
		None,
		/// <summary>
		/// Detect and include job separator and continue scanning.
		/// </summary>
		IncludeContinue,
		/// <summary>
		/// Detect and include job separator and stop scanning.
		/// </summary>
		IncludeStop,
		/// <summary>
		/// Detect and exclude job separator and continue scanning.
		/// </summary>
		ExcludeContinue,
		/// <summary>
		/// Detect and exclude job separator and stop scanning.
		/// </summary>
		ExcludeStop
	}
	/// <summary>
	/// ICapJpegQuality values.
	/// Corresponds to TWJQ_* values.
	/// </summary>
	public enum JpegQuality : short
	{
		Invalid = 0,
		Unknown = -4,
		Low = -3,
		Medium = -2,
		High = -1
	}
	/// <summary>
	/// ICapLightPath values.
	/// Corresponds to TWLP_* values.
	/// </summary>
	public enum LightPath : ushort
	{
		Reflective,
		Transmissive
	}
	/// <summary>
	/// ICapLightSource values.
	/// Corresponds to TWLS_* values.
	/// </summary>
	public enum LightSource : ushort
	{
		Red,
		Green,
		Blue,
		None,
		White,
		UV,
		IR
	}
	/// <summary>
	/// TWEI_MAGTYPE values.
	/// Corresponds to TWMD_* values.
	/// </summary>
	public enum MagType : ushort
	{
		Micr,
		Raw,
		Invalid
	}
	/// <summary>
	/// ICapNoiseFilter values.
	/// Corresponds to TWNF_* values.
	/// </summary>
	public enum NoiseFilter : ushort
	{
		None,
		Auto,
		LonePixel,
		MajorityRule
	}
	/// <summary>
	/// ICapOrientation values.
	/// Corresponds to TWOR_* values.
	/// </summary>
	public enum OrientationType : ushort
	{
		Rot0 = 0,
		Rot90 = 1,
		Rot180 = 2,
		Rot270 = 3,
		Portrait = Rot0,
		Landscape = Rot270,
		Auto = 4,
		AutoTet = 5,
		AutoPicture = 6
	}
	/// <summary>
	/// ICapOverscan values.
	/// Corresponds to TWOV_* values.
	/// </summary>
	public enum OverScan : ushort
	{
		None,
		Auto,
		TopBottom,
		LeftRight,
		All
	}
	/// <summary>
	/// <see cref="P:NTwain.Data.TWPalette8.PaletteType" /> values.
	/// Corresponds to TWPA_* values.
	/// </summary>
	public enum PaletteType : ushort
	{
		Rgb,
		Gray,
		Cmy
	}
	/// <summary>
	/// ICapPlanarChunky values.
	/// Corresponds to TWPC_* values.
	/// </summary>
	public enum PlanarChunky : ushort
	{
		Chunky,
		Planar
	}
	/// <summary>
	/// ICapPatchCodeSearchPriorities,
	/// ICapSupportedPatchCodeTypes values.
	/// Corresponds to TWPCH_* values.
	/// </summary>
	public enum PatchCode : ushort
	{
		Patch1,
		Patch2,
		Patch3,
		Patch4,
		Patch6,
		PatchT
	}
	/// <summary>
	/// ICapPixelFlavor, ICapPixelFlavorCodes values.
	/// Corresponds to TWPF_* values.
	/// </summary>
	public enum PixelFlavor : ushort
	{
		Chocolate,
		Vanilla
	}
	/// <summary>
	/// CapPrinterMode values.
	/// Corresponds to TWPM_* values.
	/// </summary>
	public enum PrinterMode : ushort
	{
		SingleString,
		MultiString,
		CompoundString
	}
	/// <summary>
	/// CapPrinter values.
	/// Corresponds to TWPR_* values.
	/// </summary>
	public enum Printer : ushort
	{
		ImprinterTopBefore,
		ImprinterTopAfter,
		ImprinterBottomBefore,
		ImprinterBottomAfter,
		EndorserTopBefore,
		EndorserTopAfter,
		EndorserBottomBefore,
		EndorserBottomAfter
	}
	/// <summary>
	/// CapPrinterFontStyle values.
	/// Corresponds to TWPF_* values.
	/// </summary>
	public enum PrinterFontStyle : ushort
	{
		Normal,
		Bold,
		Italic,
		LargeSize,
		SmallSize
	}
	/// <summary>
	/// CapPrinterIndexTrigger values.
	/// Corresponds to TWCT_* values.
	/// </summary>
	public enum PrinterIndexTrigger : ushort
	{
		Page,
		Patch1,
		Patch2,
		Patch3,
		Patch4,
		PatchT,
		Patch6
	}
	/// <summary>
	/// CapPowerSupply values.
	/// Corresponds to TWPS_* values.
	/// </summary>
	public enum PowerSupply : ushort
	{
		External,
		Battery
	}
	/// <summary>
	/// Also ICapAutomaticColorNonColorPixelType (bw/gray),
	/// ICapJpegPixelType, ICapPixelType values.
	/// Corresponds to TWPT_* values.
	/// </summary>
	public enum PixelType : ushort
	{
		BlackWhite = 0,
		Gray = 1,
		RGB = 2,
		Palette = 3,
		CMY = 4,
		CMYK = 5,
		YUV = 6,
		YUVK = 7,
		CieXYZ = 8,
		Lab = 9,
		SRGB = 10,
		SCRGB = 11,
		Infrared = 16
	}
	/// <summary>
	/// CapSegmented values.
	/// Corresponds to TWSG_* values.
	/// </summary>
	public enum Segmented : ushort
	{
		None,
		Auto,
		Manual
	}
	/// <summary>
	/// ICapFilmType values.
	/// Corresponds to TWFM_* values.
	/// </summary>
	public enum FilmType : ushort
	{
		Positive,
		Negative
	}
	/// <summary>
	/// CapDoubleFeedDetection values.
	/// Corresponds to TWDF_* values.
	/// </summary>
	public enum DoubleFeedDetection : ushort
	{
		Ultrasonic,
		ByLength,
		Infrared
	}
	/// <summary>
	/// CapDoubleFeedDetectionSensitivity values.
	/// Corresponds to TWUS_* values.
	/// </summary>
	public enum DoubleFeedDetectionSensitivity : ushort
	{
		Low,
		Medium,
		High
	}
	/// <summary>
	/// CapDoubleFeedDetectionResponse values.
	/// Corresponds to TWDP_* values.
	/// </summary>
	public enum DoubleFeedDetectionResponse : ushort
	{
		Stop,
		StopAndWait,
		Sound,
		DoNotImprint
	}
	/// <summary>
	/// ICapMirror values.
	/// Corresponds to TWMR* values.
	/// </summary>
	public enum Mirror : ushort
	{
		None,
		Vertical,
		Horizontal
	}
	/// <summary>
	/// ICapJpegSubsampling values.
	/// Corresponds to TWJS_* values.
	/// </summary>
	public enum JpegSubsampling : ushort
	{
		x444YCBCR,
		x444RGB,
		x422,
		x421,
		x411,
		x420,
		x410,
		x311
	}
	/// <summary>
	/// CapPaperHandling values.
	/// Corresponds to TWPH_* values.
	/// </summary>
	public enum PaperHandling : ushort
	{
		Normal,
		Fragile,
		Thick,
		Trifold,
		Photograph
	}
	/// <summary>
	/// CapIndicatorsMode values.
	/// Corresponds to TWCI_* values.
	/// </summary>
	public enum IndicatorsMode : ushort
	{
		Info,
		Warning,
		Error,
		Warmup
	}
	/// <summary>
	/// ICapSupportedSizes values.
	/// Corresponds to TWSS_* values.
	/// </summary>
	public enum SupportedSize : ushort
	{
		None = 0,
		A4 = 1,
		JisB5 = 2,
		USLetter = 3,
		USLegal = 4,
		A5 = 5,
		IsoB4 = 6,
		IsoB6 = 7,
		USLedger = 9,
		USExecutive = 10,
		A3 = 11,
		IsoB3 = 12,
		A6 = 13,
		C4 = 14,
		C5 = 15,
		C6 = 16,
		FourA0 = 17,
		TwoA0 = 18,
		A0 = 19,
		A1 = 20,
		A2 = 21,
		A7 = 22,
		A8 = 23,
		A9 = 24,
		A10 = 25,
		IsoB0 = 26,
		IsoB1 = 27,
		IsoB2 = 28,
		IsoB5 = 29,
		IsoB7 = 30,
		IsoB8 = 31,
		IsoB9 = IsoB8,
		IsoB10 = 33,
		JisB0 = 34,
		JisB1 = 35,
		JisB2 = 36,
		JisB3 = 37,
		JisB4 = 38,
		JisB6 = 39,
		JisB7 = 40,
		JisB8 = 41,
		JisB9 = 42,
		JisB10 = 43,
		C0 = 44,
		C1 = 45,
		C2 = 46,
		C3 = 47,
		C7 = 48,
		C8 = 49,
		C9 = 50,
		C10 = 51,
		USStatement = 52,
		BusinessCard = 53,
		MaxSize = 54
	}
	/// <summary>
	/// ICapXferMech, ACapXferMech values.
	/// Corresponds to TWSX_* values.
	/// </summary>
	public enum XferMech : ushort
	{
		Native = 0,
		File = 1,
		Memory = 2,
		MemFile = 4
	}
	/// <summary>
	/// ICapUnits values.
	/// Corresponds to TWUN_* values.
	/// </summary>
	public enum Unit : ushort
	{
		Inches,
		Centimeters,
		Picas,
		Points,
		Twips,
		Pixels,
		Millimeters
	}
	/// <summary>
	/// Corresponds to TWCY_* values.
	/// </summary>
	public enum Country : ushort
	{
		None = 0,
		Afghanistan = 1001,
		Algeria = 213,
		AmericanSamoa = 684,
		Andorra = 33,
		Angola = 1002,
		Anguilla = 8090,
		Antigua = 8091,
		Argentina = 54,
		Aruba = 297,
		AscensionIsland = 247,
		Australia = 61,
		Austria = 43,
		Bahamas = 8092,
		Bahrain = 973,
		Bangladesh = 880,
		Barbados = 8093,
		Belgium = 32,
		Belize = 501,
		Benin = 229,
		Bermuda = 8094,
		Bhutan = 1003,
		Bolivia = 591,
		Botswana = 267,
		Britain = 6,
		BritishVirginIslands = 8095,
		Brazil = 55,
		Brunei = 673,
		Bulgaria = 359,
		BurkinaFaso = 1004,
		Burma = 1005,
		Burundi = 1006,
		Camaroon = 237,
		Canada = 2,
		CapeVerdeIslands = 238,
		CaymanIslands = 8096,
		CentralAfricanRepublic = 1007,
		Chad = 1008,
		Chile = 56,
		China = 86,
		ChristmasIsland = 1009,
		CocosIslands = ChristmasIsland,
		Colombia = 57,
		Comoros = 1010,
		Congo = 1011,
		CookIs = 1012,
		Costarica = 506,
		Cuba = 5,
		Cyprus = 357,
		Czechoslovakia = 42,
		Denmark = 45,
		Djibouti = 1013,
		Dominica = 8097,
		DomincanRep = 8098,
		EasterIs = 1014,
		Ecuador = 593,
		Egypt = 20,
		ElSalvador = 503,
		Eqguinea = 1015,
		Ethiopia = 251,
		FalklandIs = 1016,
		FaeroeIs = 298,
		FijiIslands = 679,
		Finland = 358,
		France = Andorra,
		Frantilles = 596,
		Frguiana = 594,
		Frpolyneisa = 689,
		Futanais = 1043,
		Gabon = 241,
		Gambia = 220,
		Germany = 49,
		Ghana = 233,
		Gibralter = 350,
		Greece = 30,
		Greenland = 299,
		Grenada = 8099,
		Grenedines = 8015,
		Guadeloupe = 590,
		Guam = 671,
		GuantanamoBay = 5399,
		Guatemala = 502,
		Guinea = 224,
		GuineaBissau = 1017,
		Guyana = 592,
		Haiti = 509,
		Honduras = 504,
		HongKong = 852,
		Hungary = 36,
		Iceland = 354,
		India = 91,
		Indonesia = 62,
		Iran = 98,
		Iraq = 964,
		Ireland = 353,
		Israel = 972,
		Italy = 39,
		IvoryCoast = 225,
		Jamaica = 8010,
		Japan = 81,
		Jordan = 962,
		Kenya = 254,
		Kiribati = 1018,
		Korea = 82,
		Kuwait = 965,
		Laos = 1019,
		Lebanon = 1020,
		Liberia = 231,
		Libya = 218,
		Liechtenstein = 41,
		Luxenbourg = 352,
		Macao = 853,
		Madagascar = 1021,
		Malawi = 265,
		Malaysia = 60,
		Maldives = 960,
		Mali = 1022,
		Malta = 356,
		MarshallIs = 692,
		Mauritania = 1023,
		Mauritius = 230,
		Mexico = 3,
		Micronesia = 691,
		Miquelon = 508,
		Monaco = Andorra,
		Mongolia = 1024,
		Montserrat = 8011,
		Morocco = 212,
		Mozambique = 1025,
		Namibia = 264,
		Nauru = 1026,
		Nepal = 977,
		Netherlands = 31,
		NethAntilles = 599,
		Nevis = 8012,
		NewCaledonia = 687,
		NewZealand = 64,
		Nicaragua = 505,
		Niger = 227,
		Nigeria = 234,
		Niue = 1027,
		NorfolkI = 1028,
		Norway = 47,
		Oman = 968,
		Pakistan = 92,
		Palau = 1029,
		Panama = 507,
		Paraguay = 595,
		Peru = 51,
		Phillippines = 63,
		PitcairnIs = 1030,
		PNewGuinea = 675,
		Poland = 48,
		Portugal = 351,
		Qatar = 974,
		Reunioni = 1031,
		Romania = 40,
		Rwanda = 250,
		Saipan = 670,
		SanMarino = Italy,
		Saotome = 1033,
		SaudiArabia = 966,
		Senegal = 221,
		Seychellesis = 1034,
		Sierraleone = 1035,
		Singapore = 65,
		Solomonis = 1036,
		Somali = 1037,
		SouthAfrica = 27,
		Spain = 34,
		Srilanka = 94,
		Sthelena = 1032,
		Stkitts = 8013,
		Stlucia = 8014,
		Stpierre = Miquelon,
		Stvincent = Grenedines,
		Sudan = 1038,
		Suriname = 597,
		Swaziland = 268,
		Sweden = 46,
		Switzerland = Liechtenstein,
		Syria = 1039,
		Taiwan = 886,
		Tanzania = 255,
		Thailand = 66,
		Tobago = 8016,
		Togo = 228,
		Tongais = 676,
		Trinidad = Tobago,
		Tunisia = 216,
		Turkey = 90,
		TurksCaicos = 8017,
		Tuvalu = 1040,
		Uganda = 256,
		Ussr = 7,
		UAEmirates = 971,
		UnitedKingdom = 44,
		Usa = 1,
		Uruguay = 598,
		Vanuatu = 1041,
		VaticanCity = Italy,
		Venezuela = 58,
		Wake = 1042,
		WallisIs = Futanais,
		WesternSahara = 1044,
		WesternSamoa = 1045,
		Yemen = 1046,
		Yugoslavia = 38,
		Zaire = 243,
		Zambia = 260,
		Zimbabwe = 263,
		Albania = 355,
		Armenia = 374,
		Azerbaijan = 994,
		Belarus = 375,
		BosniaHerzgo = 387,
		Cambodia = 855,
		Croatia = 385,
		CzechRepublic = 420,
		Diegogarcia = 246,
		Eritrea = 291,
		Estonia = 372,
		Georgia = 995,
		Latvia = 371,
		Lesotho = 266,
		Lithuania = 370,
		Macedonia = 389,
		Mayotteis = 269,
		Moldova = 373,
		Myanmar = 95,
		NorthKorea = 850,
		PuertoRico = 787,
		Russia = Ussr,
		Serbia = 381,
		Slovakia = 421,
		Slovenia = 386,
		SouthKorea = Korea,
		Ukraine = 380,
		USVirginIs = 340,
		Vietnam = 84
	}
	/// <summary>
	/// Corresponds to TWLG_* values.
	/// </summary>
	public enum Language : short
	{
		UserLocale = -1,
		Danish,
		Dutch,
		English,
		FrenchCanadian,
		Finnish,
		French,
		German,
		Icelandic,
		Italian,
		Norwegian,
		Portuguese,
		Spanish,
		Swedish,
		EnglishUSA,
		Afrikaans,
		Albania,
		Arabic,
		ArabicAlgeria,
		ArabicBahrain,
		ArabicEgypt,
		ArabicIraq,
		ArabicJordan,
		ArabicKuwait,
		ArabicLebanon,
		ArabicLibya,
		ArabicMorocco,
		ArabicOman,
		ArabicQatar,
		ArabicSaudiArabia,
		ArabicSyria,
		ArabicTunisia,
		ArabicUAE,
		ArabicYemen,
		Basque,
		Byelorussian,
		Bulgarian,
		Catalan,
		Chinese,
		ChineseHongKong,
		ChinesePRC,
		ChineseSingapore,
		ChineseSimplified,
		ChineseTaiwan,
		ChineseTraditional,
		Croatia,
		Czech,
		DutchBelgian,
		EnglishAustralian,
		EnglishCanadian,
		EnglishIreland,
		EnglishNewZealand,
		EnglishSouthAfrica,
		EnglishUK,
		Estonian,
		Faeroese,
		Farsi,
		FrenchBelgian,
		FrenchLuxembourg,
		FrenchSwiss,
		GermanAustrian,
		GermanLuxembourg,
		GermanLiechtenstein,
		GermanSwiss,
		Greek,
		Hebrew,
		Hungarian,
		Indonesian,
		ItalianSwiss,
		Japanese,
		Korean,
		KoreanJohab,
		Latvian,
		Lithuanian,
		NorwegianBokmal,
		NorwegianNynorsk,
		Polish,
		PortugueseBrazil,
		Romanian,
		Russian,
		SerbianLatin,
		Slovak,
		Slovenian,
		SpanishMexican,
		SpanishModern,
		Thai,
		Turkish,
		Ukranian,
		Assamese,
		Bengali,
		Bihari,
		Bodo,
		Dogri,
		Gujarati,
		Haryanvi,
		Hindi,
		Kannada,
		Kashmiri,
		Malayalam,
		Marathi,
		Marwari,
		Meghalayan,
		Mizo,
		Naga,
		Orissi,
		Punjabi,
		Pushtu,
		SerbianCyrillic,
		Sikkimi,
		SwedishFinland,
		Tamil,
		Telugu,
		Tripuri,
		Urdu,
		Vietnamese
	}
	/// <summary>
	/// Corresponds to DG_*.
	/// </summary>
	[Flags]
	public enum DataGroups : uint
	{
		None = 0u,
		Control = 1u,
		Image = 2u,
		Audio = 4u,
		Mask = 0xFFFFu
	}
	/// <summary>
	/// Corresponds to DF_*.
	/// </summary>
	[Flags]
	public enum DataFunctionalities : uint
	{
		None = 0u,
		Dsm2 = 0x10000000u,
		App2 = 0x20000000u,
		Ds2 = 0x40000000u
	}
	/// <summary>
	/// Corresponds to DAT_*.
	/// </summary>
	public enum DataArgumentType : ushort
	{
		Null = 0,
		CustomBase = 32768,
		Capability = 1,
		Event = 2,
		Identity = 3,
		Parent = 4,
		PendingXfers = 5,
		SetupMemXfer = 6,
		SetupFileXfer = 7,
		Status = 8,
		UserInterface = 9,
		XferGroup = 10,
		CustomDSData = 12,
		DeviceEvent = 13,
		FileSystem = 14,
		PassThru = 15,
		Callback = 16,
		StatusUtf8 = 17,
		Callback2 = 18,
		ImageInfo = 257,
		ImageLayout = 258,
		ImageMemXfer = 259,
		ImageNativeXfer = 260,
		ImageFileXfer = 261,
		CieColor = 262,
		GrayResponse = 263,
		RgbResponse = 264,
		JpegCompression = 265,
		Palette8 = 266,
		ExtImageInfo = 267,
		Filter = 268,
		AudioFileXfer = 513,
		AudioInfo = 514,
		AudioNativeXfer = 515,
		IccProfile = 1025,
		ImageMemFileXfer = 1026,
		EntryPoint = 1027
	}
	/// <summary>
	/// Corresponds to MSG_*.
	/// </summary>
	public enum Message : ushort
	{
		Null = 0,
		CustomBase = 32768,
		Get = 1,
		GetCurrent = 2,
		GetDefault = 3,
		GetFirst = 4,
		GetNext = 5,
		Set = 6,
		Reset = 7,
		QuerySupport = 8,
		GetHelp = 9,
		GetLabel = 10,
		GetLabelEnum = 11,
		SetConstraint = 12,
		XferReady = 257,
		CloseDSReq = 258,
		CloseDSOK = 259,
		DeviceEvent = 260,
		OpenDSM = 769,
		CloseDSM = 770,
		OpenDS = 1025,
		CloseDS = 1026,
		UserSelect = 1027,
		DisableDS = 1281,
		EnableDS = 1282,
		EnableDSUIOnly = 1283,
		ProcessEvent = 1537,
		EndXfer = 1793,
		StopFeeder = 1794,
		ChangeDirectory = 2049,
		CreateDirectory = 2050,
		Delete = 2051,
		FormatMedia = 2052,
		GetClose = 2053,
		GetFirstFile = 2054,
		GetInfo = 2055,
		GetNextFile = 2056,
		Rename = 2057,
		Copy = 2058,
		AutomaticCaptureDirectory = 2059,
		PassThru = 2305,
		RegisterCallback = 2306,
		ResetAll = 2561
	}
	/// <summary>
	/// Indicates the type of capability.
	/// </summary>
	public enum CapabilityId : ushort
	{
		None = 0,
		CustomBase = 32768,
		CapXferCount = 1,
		ICapCompression = 256,
		ICapPixelType = 257,
		ICapUnits = 258,
		ICapXferMech = 259,
		CapAuthor = 4096,
		CapCaption = 4097,
		CapFeederEnabled = 4098,
		CapFeederLoaded = 4099,
		CapTimeDate = 4100,
		CapSupportedCaps = 4101,
		CapExtendedCaps = 4102,
		CapAutoFeed = 4103,
		CapClearPage = 4104,
		CapFeedPage = 4105,
		CapRewindPage = 4106,
		CapIndicators = 4107,
		CapPaperDetectable = 4109,
		CapUIControllable = 4110,
		CapDeviceOnline = 4111,
		CapAutoScan = 4112,
		CapThumbnailsEnabled = 4113,
		CapDuplex = 4114,
		CapDuplexEnabled = 4115,
		CapEnableDSUIOnly = 4116,
		CapCustomDSData = 4117,
		CapEndorser = 4118,
		CapJobControl = 4119,
		CapAlarms = 4120,
		CapAlarmVolume = 4121,
		CapAutomaticCapture = 4122,
		CapTimeBeforeFirstCapture = 4123,
		CapTimeBetweenCaptures = 4124,
		CapClearBuffers = 4125,
		CapMaxBatchBuffers = 4126,
		CapDeviceTimeDate = 4127,
		CapPowerSupply = 4128,
		CapCameraPreviewUI = 4129,
		CapDeviceEvent = 4130,
		CapSerialNumber = 4132,
		CapPrinter = 4134,
		CapPrinterEnabled = 4135,
		CapPrinterIndex = 4136,
		CapPrinterMode = 4137,
		CapPrinterString = 4138,
		CapPrinterSuffix = 4139,
		CapLanguage = 4140,
		CapFeederAlignment = 4141,
		CapFeederOrder = 4142,
		CapReacquireAllowed = 4144,
		CapBatteryMinutes = 4146,
		CapBatteryPercentage = 4147,
		CapCameraSide = 4148,
		CapSegmented = 4149,
		CapCameraEnabled = 4150,
		CapCameraOrder = 4151,
		CapMicrEnabled = 4152,
		CapFeederPrep = 4153,
		CapFeederPocket = 4154,
		CapAutomaticSenseMedium = 4155,
		CapCustomInterfaceGuid = 4156,
		CapSupportedCapsSegmentUnique = 4157,
		CapSupportedDATs = 4158,
		CapDoubleFeedDetection = 4159,
		CapDoubleFeedDetectionLength = 4160,
		CapDoubleFeedDetectionSensitivity = 4161,
		CapDoubleFeedDetectionResponse = 4162,
		CapPaperHandling = 4163,
		CapIndicatorsMode = 4164,
		CapPrinterVerticalOffset = 4165,
		CapPowerSaveTime = 4166,
		CapPrinterCharRotation = 4167,
		CapPrinterFontStyle = 4168,
		CapPrinterIndexLeadChar = 4169,
		CapPrinterIndexMaxValue = 4170,
		CapPrinterIndexNumDigits = 4171,
		CapPrinterIndexStep = 4172,
		CapPrinterIndexTrigger = 4173,
		CapPrinterStringPreview = 4174,
		ICapAutoBright = 4352,
		ICapBrightness = 4353,
		ICapContrast = 4355,
		ICapCustHalftone = 4356,
		ICapExposureTime = 4357,
		ICapFilter = 4358,
		[Obsolete("Use ICapFlashUsed2 instead.")]
		ICapFlashUsed = 4359,
		ICapGamma = 4360,
		ICapHalftones = 4361,
		ICapHighlight = 4362,
		ICapImageFileFormat = 4364,
		ICapLampState = 4365,
		ICapLightSource = 4366,
		ICapOrientation = 4368,
		ICapPhysicalWidth = 4369,
		ICapPhysicalHeight = 4370,
		ICapShadow = 4371,
		ICapFrames = 4372,
		ICapXNativeResolution = 4374,
		ICapYNativeResolution = 4375,
		ICapXResolution = 4376,
		ICapYResolution = 4377,
		ICapMaxFrames = 4378,
		ICapTiles = 4379,
		ICapBitOrder = 4380,
		ICapCCITTKFactor = 4381,
		ICapLightPath = 4382,
		ICapPixelFlavor = 4383,
		ICapPlanarChunky = 4384,
		ICapRotation = 4385,
		ICapSupportedSizes = 4386,
		ICapThreshold = 4387,
		ICapXScaling = 4388,
		ICapYScaling = 4389,
		ICapBitOrderCodes = 4390,
		ICapPixelFlavorCodes = 4391,
		ICapJpegPixelType = 4392,
		ICapTimeFill = 4394,
		ICapBitDepth = 4395,
		ICapBitDepthReduction = 4396,
		ICapUndefinedImageSize = 4397,
		ICapImageDataSet = 4398,
		ICapExtImageInfo = 4399,
		ICapMinimumHeight = 4400,
		ICapMinimumWidth = 4401,
		ICapAutoDiscardBlankPages = 4404,
		ICapFlipRotation = 4406,
		ICapBarcodeDetectionEnabled = 4407,
		ICapSupportedBarcodeTypes = 4408,
		ICapBarcodeMaxSearchPriorities = 4409,
		ICapBarcodeSearchPriorities = 4410,
		ICapBarcodeSearchMode = 4411,
		ICapBarcodeMaxRetries = 4412,
		ICapBarcodeTimeout = 4413,
		ICapZoomFactor = 4414,
		ICapPatchCodeDetectionEnabled = 4415,
		ICapSupportedPatchCodeTypes = 4416,
		ICapPatchCodeMaxSearchPriorities = 4417,
		ICapPatchCodeSearchPriorities = 4418,
		ICapPatchCodeSearchMode = 4419,
		ICapPatchCodeMaxRetries = 4420,
		ICapPatchCodeTimeout = 4421,
		ICapFlashUsed2 = 4422,
		ICapImageFilter = 4423,
		ICapNoiseFilter = 4424,
		ICapOverScan = 4425,
		ICapAutomaticBorderDetection = 4432,
		ICapAutomaticDeskew = 4433,
		ICapAutomaticRotate = 4434,
		ICapJpegQuality = 4435,
		ICapFeederType = 4436,
		ICapICCProfile = 4437,
		ICapAutoSize = 4438,
		ICapAutomaticCropUsesFrame = 4439,
		ICapAutomaticLengthDetection = 4440,
		ICapAutomaticColorEnabled = 4441,
		ICapAutomaticColorNonColorPixelType = 4442,
		ICapColorManagementEnabled = 4443,
		ICapImageMerge = 4444,
		ICapImageMergeHeightThreshold = 4445,
		ICapSupportedExtImageInfo = 4446,
		ICapFilmType = 4447,
		ICapMirror = 4448,
		ICapJpegSubsampling = 4449,
		ACapXferMech = 4610
	}
	/// <summary>
	/// Extended Image Info Attributes.
	/// Corresponds to TWEI_*.
	/// </summary>
	public enum ExtendedImageInfo : ushort
	{
		Invalid = 0,
		BarcodeX = 4608,
		BarcodeY = 4609,
		BarcodeText = 4610,
		BarcodeType = 4611,
		DeshadeTop = 4612,
		DeshadeLeft = 4613,
		DeshadeHeight = 4614,
		DeshadeWidth = 4615,
		DeshadeSize = 4616,
		SpecklesRemoved = 4617,
		HorzLineXCoord = 4618,
		HorzLineYCoord = 4619,
		HorzLineLength = 4620,
		HorzLineThickness = 4621,
		VertLineXCoord = 4622,
		VertLineYCoord = 4623,
		VertLineLength = 4624,
		VertLineThickness = 4625,
		PatchCode = 4626,
		EndorsedText = 4627,
		FormConfidence = 4628,
		FormTemplateMatch = 4629,
		FormTemplatePageMatch = 4630,
		FormHorzDocOffset = 4631,
		FormVertDocOffset = 4632,
		BarcodeCount = 4633,
		BarcodeConfidence = 4634,
		BarcodeRotation = 4635,
		BarcodeTextLength = 4636,
		DeshadeCount = 4637,
		DeshadeBlackCountOld = 4638,
		DeshadeBlackCountNew = 4639,
		DeshadeBlackRLMin = 4640,
		DeshadeBlackRLMax = 4641,
		DeshadeWhiteCountOld = 4642,
		DeshadeWhiteCountNew = 4643,
		DeshadeWhiteRLMin = 4644,
		DeshadeWhiteRLAve = 4645,
		DeshadeWhiteRLMax = 4646,
		BlackSpecklesRemoved = 4647,
		WhiteSpecklesRemoved = 4648,
		HorzLineCount = 4649,
		VertLineCount = 4650,
		DeskewStatus = 4651,
		SkewOriginalAngle = 4652,
		SkewFinalAngle = 4653,
		SkewConfidence = 4654,
		SkewWindowX1 = 4655,
		SkewWindowY1 = 4656,
		SkewWindowX2 = 4657,
		SkewWindowY2 = 4658,
		SkewWindowX3 = 4659,
		SkewWindowY3 = 4660,
		SkewWindowX4 = 4661,
		SkewWindowY4 = 4662,
		BookName = 4664,
		ChapterNumber = 4665,
		DocumentNumber = 4666,
		PageNumber = 4667,
		Camera = 4668,
		FrameNumber = 4669,
		Frame = 4670,
		PixelFlavor = 4671,
		IccProfile = 4672,
		LastSegment = 4673,
		SegmentNumber = 4674,
		MagData = 4675,
		MagType = 4676,
		PageSide = 4677,
		FileSystemSource = 4678,
		ImageMerged = 4679,
		MagDataLength = 4680,
		PaperCount = 4681,
		PrinterText = 4682
	}
	/// <summary>
	/// EndXfer job control values.
	/// Corresponds to TWEJ_*.
	/// </summary>
	public enum EndXferJob : ushort
	{
		None,
		MidSeparator,
		Patch1,
		Patch2,
		Patch3,
		Patch4,
		Patch6,
		PatchT
	}
	/// <summary>
	/// Corresponds to TWRC_*.
	/// </summary>
	public enum ReturnCode : ushort
	{
		CustomBase = 32768,
		Success = 0,
		Failure = 1,
		CheckStatus = 2,
		Cancel = 3,
		DSEvent = 4,
		NotDSEvent = 5,
		XferDone = 6,
		EndOfList = 7,
		InfoNotSupported = 8,
		DataNotAvailable = 9,
		Busy = 10,
		ScannerLocked = 11
	}
	/// <summary>
	/// Corresponds to TWCC_*.
	/// </summary>
	public enum ConditionCode : ushort
	{
		CustomBase = 32768,
		Success = 0,
		Bummer = 1,
		LowMemory = 2,
		NoDS = 3,
		MaxConnections = 4,
		OperationError = 5,
		BadCap = 6,
		BadProtocol = 9,
		BadValue = 10,
		SeqError = 11,
		BadDest = 12,
		CapUnsupported = 13,
		CapBadOperation = 14,
		CapSeqError = 15,
		Denied = 16,
		FileExists = 17,
		FileNotFound = 18,
		NotEmpty = 19,
		PaperJam = 20,
		PaperDoubleFeed = 21,
		FileWriteError = 22,
		CheckDeviceOnline = 23,
		Interlock = 24,
		DamagedCorner = 25,
		FocusError = 26,
		DocTooLight = 27,
		DocTooDark = 28,
		NoMedia = 29
	}
	/// <summary>
	/// Bit mask for querying the operation that are supported by the data source on a capability.
	/// Corresponds to TWQC_*.
	/// </summary>
	[Flags]
	public enum QuerySupports
	{
		None = 0,
		Get = 1,
		Set = 2,
		GetDefault = 4,
		GetCurrent = 8,
		Reset = 0x10,
		SetConstraint = 0x20,
		Constrainable = 0x40,
		GetHelp = 0x100,
		GetLabel = 0x200,
		GetLabelEnum = 0x400,
		/// <summary>
		/// Cap applies to entire session/machine.
		/// </summary>
		Machine = 0x1000,
		/// <summary>
		/// Cap applies to bitonal cameras.
		/// </summary>
		Bitonal = 0x2000,
		/// <summary>
		/// Cap applies to color cameras.
		/// </summary>
		Color = 0x4000
	}
	/// <summary>
	/// TWAIN's boolean values.
	/// </summary>
	public enum BoolType : ushort
	{
		/// <summary>
		/// The false value (0).
		/// </summary>
		False,
		/// <summary>
		/// The true value (1).
		/// </summary>
		True
	}
	/// <summary>
	/// Contains direct magic values for some TWAIN stuff.
	/// </summary>
	public static class TwainConst
	{
		/// <summary>
		/// Length of an array that holds TW_STR32.
		/// </summary>
		public const int String32 = 34;

		/// <summary>
		/// Length of an array that holds TW_STR64.
		/// </summary>
		public const int String64 = 66;

		/// <summary>
		/// Length of an array that holds TW_STR128.
		/// </summary>
		public const int String128 = 130;

		/// <summary>
		/// Length of an array that holds TW_STR255.
		/// </summary>
		public const int String255 = 256;

		/// <summary>
		/// Don't care value for 8 bit types.
		/// </summary>
		public const byte DoNotCare8 = byte.MaxValue;

		/// <summary>
		/// Don't care value for 16 bit types.
		/// </summary>
		public const ushort DoNotCare16 = ushort.MaxValue;

		/// <summary>
		/// Don't care value for 32 bit types.
		/// </summary>
		public const uint DoNotCare32 = uint.MaxValue;

		/// <summary>
		/// The major version number of TWAIN supported by this library.
		/// </summary>
		public const short ProtocolMajor = 2;

		/// <summary>
		/// The minor version number of TWAIN supported by this library.
		/// </summary>
		public const short ProtocolMinor = 3;

		/// <summary>
		/// Value for false where applicable.
		/// </summary>
		public const ushort True = 1;

		/// <summary>
		/// Value for true where applicable.
		/// </summary>
		public const ushort False = 0;
	}
	/// <summary>
	/// Contains extensions for reading/writing TWAIN types from/to pointer.
	/// </summary>
	public static class TypeExtensions
	{
		private static readonly IDictionary<ItemType, int> __sizes = new Dictionary<ItemType, int>
		{
			{
				ItemType.Int8,
				1
			},
			{
				ItemType.UInt8,
				1
			},
			{
				ItemType.Int16,
				2
			},
			{
				ItemType.UInt16,
				2
			},
			{
				ItemType.Bool,
				2
			},
			{
				ItemType.Int32,
				4
			},
			{
				ItemType.UInt32,
				4
			},
			{
				ItemType.Fix32,
				4
			},
			{
				ItemType.Frame,
				16
			},
			{
				ItemType.String32,
				34
			},
			{
				ItemType.String64,
				66
			},
			{
				ItemType.String128,
				130
			},
			{
				ItemType.String255,
				256
			},
			{
				ItemType.Handle,
				IntPtr.Size
			}
		};

		/// <summary>
		/// Gets the byte size of the item type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static int GetItemTypeSize(this ItemType type)
		{
			if (__sizes.ContainsKey(type))
			{
				return __sizes[type];
			}
			return 0;
		}

		/// <summary>
		/// Reads a TWAIN value.
		/// </summary>
		/// <param name="baseAddress">The base address.</param>
		/// <param name="offset">The offset.</param>
		/// <param name="type">The TWAIN type.</param>
		/// <returns></returns>
		public static object ReadValue(this IntPtr baseAddress, ref int offset, ItemType type)
		{
			object result = null;
			switch (type)
			{
			case ItemType.Int8:
				result = (sbyte)Marshal.ReadByte(baseAddress, offset);
				break;
			case ItemType.UInt8:
				result = Marshal.ReadByte(baseAddress, offset);
				break;
			case ItemType.UInt16:
			case ItemType.Bool:
				result = (ushort)Marshal.ReadInt16(baseAddress, offset);
				break;
			case ItemType.Int16:
				result = Marshal.ReadInt16(baseAddress, offset);
				break;
			case ItemType.UInt32:
				result = (uint)Marshal.ReadInt32(baseAddress, offset);
				break;
			case ItemType.Int32:
				result = Marshal.ReadInt32(baseAddress, offset);
				break;
			case ItemType.Fix32:
				result = new TWFix32
				{
					Whole = Marshal.ReadInt16(baseAddress, offset),
					Fraction = (ushort)Marshal.ReadInt16(baseAddress, offset + 2)
				};
				break;
			case ItemType.Frame:
				return new TWFrame
				{
					Left = (TWFix32)baseAddress.ReadValue(ref offset, ItemType.Fix32),
					Top = (TWFix32)baseAddress.ReadValue(ref offset, ItemType.Fix32),
					Right = (TWFix32)baseAddress.ReadValue(ref offset, ItemType.Fix32),
					Bottom = (TWFix32)baseAddress.ReadValue(ref offset, ItemType.Fix32)
				};
			case ItemType.String32:
			case ItemType.String64:
			case ItemType.String128:
			case ItemType.String255:
				result = Marshal.PtrToStringAnsi(new IntPtr(baseAddress.ToInt64() + offset));
				break;
			case ItemType.Handle:
				result = Marshal.ReadIntPtr(baseAddress, offset);
				break;
			}
			offset += type.GetItemTypeSize();
			return result;
		}

		/// <summary>
		/// Writes a TWAIN value.
		/// </summary>
		/// <param name="baseAddr">The base addr.</param>
		/// <param name="offset">The offset.</param>
		/// <param name="type">The TWAIN type.</param>
		/// <param name="value">The value.</param>
		public static void WriteValue(this IntPtr baseAddr, ref int offset, ItemType type, object value)
		{
			switch (type)
			{
			case ItemType.Int8:
			case ItemType.UInt8:
				Marshal.WriteByte(baseAddr, offset, Convert.ToByte(value, CultureInfo.InvariantCulture));
				break;
			case ItemType.Int16:
			case ItemType.UInt16:
			case ItemType.Bool:
				Marshal.WriteInt16(baseAddr, offset, Convert.ToInt16(value, CultureInfo.InvariantCulture));
				break;
			case ItemType.Int32:
			case ItemType.UInt32:
				Marshal.WriteInt32(baseAddr, offset, Convert.ToInt32(value, CultureInfo.InvariantCulture));
				break;
			case ItemType.Fix32:
			{
				TWFix32 tWFix = (TWFix32)value;
				Marshal.WriteInt16(baseAddr, offset, tWFix.Whole);
				if (tWFix.Fraction > 32767)
				{
					Marshal.WriteInt16(baseAddr, offset + 2, (short)(tWFix.Fraction - 32768));
				}
				else
				{
					Marshal.WriteInt16(baseAddr, offset + 2, (short)tWFix.Fraction);
				}
				break;
			}
			case ItemType.Frame:
			{
				TWFrame tWFrame = (TWFrame)value;
				baseAddr.WriteValue(ref offset, ItemType.Fix32, (TWFix32)tWFrame.Left);
				baseAddr.WriteValue(ref offset, ItemType.Fix32, (TWFix32)tWFrame.Top);
				baseAddr.WriteValue(ref offset, ItemType.Fix32, (TWFix32)tWFrame.Right);
				baseAddr.WriteValue(ref offset, ItemType.Fix32, (TWFix32)tWFrame.Bottom);
				return;
			}
			case ItemType.String128:
				WriteString(baseAddr, offset, (string)value, 128);
				break;
			case ItemType.String255:
				WriteString(baseAddr, offset, (string)value, 255);
				break;
			case ItemType.String32:
				WriteString(baseAddr, offset, (string)value, 32);
				break;
			case ItemType.String64:
				WriteString(baseAddr, offset, (string)value, 64);
				break;
			}
			offset += type.GetItemTypeSize();
		}

		/// <summary>
		/// Writes string value. THIS MAY BE WRONG.
		/// </summary>
		/// <param name="baseAddr"></param>
		/// <param name="offset"></param>
		/// <param name="item"></param>
		/// <param name="maxLength"></param>
		private static void WriteString(IntPtr baseAddr, int offset, string item, int maxLength)
		{
			if (string.IsNullOrEmpty(item))
			{
				Marshal.WriteByte(baseAddr, offset, 0);
				return;
			}
			for (int i = 0; i < maxLength; i++)
			{
				if (i == item.Length)
				{
					Marshal.WriteByte(baseAddr, offset, 0);
					return;
				}
				Marshal.WriteByte(baseAddr, offset, (byte)item[i]);
				offset++;
			}
			Marshal.WriteByte(baseAddr, offset, 0);
		}
	}
	/// <summary>
	/// Contains extension methods for converting (possibly bad) integer values to enum values.
	/// </summary>
	public static class ValueExtensions
	{
		/// <summary>
		/// Casts a list of objects to a list of specified enum.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public static IList<TEnum> CastToEnum<TEnum>(this IEnumerable<object> list) where TEnum : struct, IConvertible
		{
			return list.CastToEnum<TEnum>(tryUpperWord: true);
		}

		/// <summary>
		/// Casts a list of objects to a list of specified enum.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="list">The list.</param>
		/// <param name="tryUpperWord">set to <c>true</c> for working with bad values.</param>
		/// <returns></returns>
		public static IList<TEnum> CastToEnum<TEnum>(this IEnumerable<object> list, bool tryUpperWord) where TEnum : struct, IConvertible
		{
			return list.Select((object o) => o.ConvertToEnum<TEnum>(tryUpperWord)).ToList();
		}

		/// <summary>
		/// Casts an objects to the specified enum.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static TEnum ConvertToEnum<TEnum>(this object value) where TEnum : struct, IConvertible
		{
			return value.ConvertToEnum<TEnum>(tryUpperWord: true);
		}

		/// <summary>
		/// Casts an objects to the specified enum.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="tryUpperWord">if set to <c>true</c> [try upper word].</param>
		/// <returns></returns>
		public static TEnum ConvertToEnum<TEnum>(this object value, bool tryUpperWord) where TEnum : struct, IConvertible
		{
			if (value != null)
			{
				Type typeFromHandle = typeof(TEnum);
				if (typeFromHandle.IsEnum)
				{
					Type underlyingType = Enum.GetUnderlyingType(typeFromHandle);
					if (tryUpperWord && typeof(ushort).IsAssignableFrom(underlyingType))
					{
						uint value2 = Convert.ToUInt32(value, CultureInfo.InvariantCulture);
						ushort lowerWord = GetLowerWord(value2);
						if (!Enum.IsDefined(typeFromHandle, lowerWord))
						{
							return (TEnum)Enum.ToObject(typeFromHandle, GetUpperWord(value2));
						}
					}
					return (TEnum)Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
				}
				if (typeof(IConvertible).IsAssignableFrom(typeFromHandle))
				{
					return (TEnum)Convert.ChangeType(value, typeFromHandle, CultureInfo.InvariantCulture);
				}
				return (TEnum)value;
			}
			return default;
		}

		private static ushort GetLowerWord(uint value)
		{
			return (ushort)(value & 0xFFFF);
		}

		private static uint GetUpperWord(uint value)
		{
			return (ushort)(value >> 16);
		}

		/// <summary>
		/// Tries to convert a value to <see cref="T:NTwain.Data.TWFix32" /> if possible.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static TWFix32 ConvertToFix32(this object value)
		{
			if (value != null)
			{
				if (value is TWFix32)
				{
					return (TWFix32)value;
				}
				return Convert.ToSingle(value, CultureInfo.InvariantCulture);
			}
			return default;
		}

		/// <summary>
		/// Tries to cast a value to <see cref="T:NTwain.Data.TWFrame" /> if possible.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static TWFrame ConvertToFrame(this object value)
		{
			if (value != null && value is TWFrame)
			{
				return (TWFrame)value;
			}
			return default;
		}

		/// <summary>
		/// Converts object to string.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static string ConvertToString(this object value)
		{
			return value?.ToString();
		}
	}
}
