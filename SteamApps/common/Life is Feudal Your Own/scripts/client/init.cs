//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Variables used by client scripts & code.  The ones marked with (c)
// are accessed from code.  Variables preceeded by Pref:: are client
// preferences and stored automatically in the data/prefs.cs file
// in between sessions.
//
//    (c) Client::MissionFile             Mission file name
//    ( ) Client::Password                Password for server join

//    (?) Pref::Player::CurrentFOV
//    (?) Pref::Player::DefaultFov
//    ( ) Pref::Input::KeyboardTurnSpeed

//    (c) pref::Master[n]                 List of master servers
//    (c) pref::Net::RegionMask
//    (c) pref::Client::ServerFavoriteCount
//    (c) pref::Client::ServerFavorite[FavoriteCount]
//    .. Many more prefs... need to finish this off

// Moves, not finished with this either...
//    (c) firstPerson
//    $mv*Action...

//-----------------------------------------------------------------------------
// These are variables used to control the shell scripts and
// can be overriden by mods:
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
function initClient()
{
   echo("\n--------- Initializing " @ $appName @ ": Client Scripts ---------");

   if($cmYO && isScriptFile("scripts/yo.cs"))
      exec("scripts/yo.cs");

   // Init of CmConfiguration
   exec("scripts/cm_config.cs");
   CmConfiguration_init();

   // override the base profiles if necessary
   if(isScriptFile("art/gui/customProfiles.cs"))
      exec("art/gui/customProfiles.cs");

   // The common module provides basic client functionality
   initBaseClient();

   //CM_CHANGE
   loadMaterials(); // should be initialized before weapons

   // datablocks... at client!
   exec("core/art/datablocks/datablockExec.cs");
   exec("art/datablocks/datablockExec.cs");

	// init of substances
	SubstanceManager_init();
	exec("scripts/cm_substances.cs");

   //skills
   reloadGatheringInfo();
   if(!initSkillsManager())
   {
      error("Fatal: Can't load skills/abilities. Terminating.");
      messageBox("Error starting up", "Can't startup: failed to load skills/abilities.", Ok, Stop);
      quit();
      return;
   }

   //titles
   if(!initTitlesManager())
   {
      error("Fatal: Can't load titles. Terminating.");
      messageBox("Error starting up", "Can't startup: failed to load titles.", Ok, Stop);
      quit();
      return;
   }
   //CM_CHANGE/

   // Load up the Game GUIs
   initTooltipTemplateManager();
   exec("gui/scripts/tooltipManager.cs");
   fillTooltipTemplate();

   exec("art/gui/defaultGameProfiles.cs");
   exec("art/gui/PlayGui.gui");

   // Load up the shell GUIs
   exec("gui/forms/guiSubtitlesTheoraCtrl.gui");
   exec("gui/forms/MainMenuMultiplayerWindow.gui");
   exec("gui/forms/CreateWorldWindow.gui");
   exec("gui/forms/JoinWorldWindow.gui");
   exec("art/gui/helpDialog.gui");
   exec("gui/forms/MessageBoxPasswordDlg.gui");
   exec("gui/forms/MessageBoxDirectConnect.gui");
   exec("art/gui/joinServerDlg.gui");

   // Gui scripts
   exec("scripts/gui/playGui.cs");

//-----------------------------------------------------------------------------
//CM_Inventory
//-----------------------------------------------------------------------------
   exec("art/gui/splitStackItem.gui");
   exec("art/gui/prospectingRadiusDialog.gui");
   exec("scripts/gui/InventoryGui.cs");
   exec("scripts/gui/prospectingRadiusDialog.cs");

   exec("scripts/gui/cmCraftworkBrewingTankOptionsDlg.gui");

   exec("scripts/gui/cmAcceptDialog.gui");
//CM_Inventory/

   exec("gui/scripts/gui.cs");

   // Client scripts
   exec("./client.cs");
   exec("./game.cs");
   exec("./missionDownload.cs");
   exec("./serverConnection.cs");
   exec("./cServer.cs");

   // Load useful Materials
   exec("./shaders.cs");


   exec("scripts/client/default.bindCommands.cs");
   exec("scripts/client/horse.bindCommands.cs");

   if( isScriptFile( "scripts/client/developers.bind.cs" ) )
      exec("scripts/client/developers.bind.cs");

   if (isFile( $bindingsConfig)) // no .dso allowed
   {
      exec( $bindingsConfig);
   }
   else
   {
      exec("scripts/client/default.bind.cs");
   }
   exec("scripts/client/default.bind.update.cs");

   //loadMaterials();

   // Really shouldn't be starting the networking unless we are
   // going to connect to a remote server, or host a multi-player
   // game.
   setNetPort(0, false);

   // Copy saved script prefs into C++ code.
   setDefaultFov( $pref::Player::defaultFov );
   setFov($pref::player::CurrentFOV);
   setZoomSpeed( $pref::Player::zoomSpeed );

   StreamGroup::addSortingGroup(1, $pref::TS::renderingDistanceSmall * 2.0,  1.0);
   StreamGroup::addSortingGroup(2, $pref::TS::renderingDistanceBig * 2.0, 1.5);
   StreamGroup::addSortingGroup(3, $pref::TS::renderingDistanceHuge * 2.0, 2.0);

   //if( isFile( "./audioData.cs" ) )
   //   exec( "./audioData.cs" );

   // Start up the main menu... this is separated out into a
   // method for easier mod override.

   // if ($startWorldEditor || $startGUIEditor) {
      // // Editor GUI's will start up in the primary main.cs once
      // // engine is initialized.
      // return;
   // }

	//CM_CHANGE

	// load light prototypes
	exec("./cm_lightProto.cs");
	//exec("./cm_environment.cs");

  	// load forest data
	exec("art/forest/treeDatablocks.cs");

  // load WebGui
  exec("scripts/web/GuiBrowserWindow.gui");
  // !load WebGui

	loadTutorial();

	initManagers();
	
	initAdminLandsAbilities();

	// start music
	exec("art/sound/music/music.cs");
	//startMusic();
	//if($cmYO && isScriptFile("scripts/yo.cs"))
	//	exec("scripts/yo.cs");
	removeFolderWithAllFiles("art/Textures/Heraldry/Cache");

	schedule(256, Canvas, dumpSysInfo, "initClient");
	onInitClientDone();
	exec("mod/sf-mods/init.cs");
}
//-----------------------------------------------------------------------------

function initManagers()
{
   // YO & MMO load the rest of managers via initManagersListThreaded()
	initSoundsManager();
	initLoadGuildsClient();
	createLandsManager();
	initLoadClaimRules();
	
   initSkinManager();
   loadAttackAnimationsXml();
   initXsollaManager();
}

//-----------------------------------------------------------------------------

// redefined in YoPackage
function cmJoinDefaultChats() {
   cmChatJoin("@");	// System channel
   cmChatJoin("");	// Local channel
}
//-----------------------------------------------------------------------------

function showWelcomeMessage()
{
	if ($pref::hideWelcomeMessage != 1 && $isNewbieWorld)
	{
		cmShowMessage(697);	// Welcome to the world of Life is Feudal:MMO!&#10;&#10;If you're new to the Life is Feudal, we recommend you to follow the tutorial quests line - that will save a lot of time and frustration for you in the future!&#10;&#10;If you're a Life is Feudal seasoned player, we recommend you to go directly to the beacon of light, talk with the Ferryman inside of it and get your character transfered on the Abella. 
		$pref::hideWelcomeMessage = 1;
	}
}
//-----------------------------------------------------------------------------
