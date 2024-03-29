namespace Compukit_UK101_UWP
{
	public partial class BasicProg 
	{
		public string[] StarTrekInstructions1_32 = new string[] {
			"",
			"",
			" 5 REM SINCE THE PRINT STATEMENTS OF STARTREK",
			" 6 REM HAVE HAD TO BE CUT SHORT AND THE",
			" 7 REM INSTRUCTIONS REMOVED TO GET IT TO FIT",
			" 8 REM ON 6K OF RAM THE INSTRUCTIONS ARE",
			" 9 REM PRESENTED HERE ON A SEPARATE PROGRAM",
			" 10 FORI=1TO16:PRINT:NEXT",
			" 20 PRINT\"      INSTRUCTIONS FOR STARTREK\"",
			" 30 PRINT\"      -------------------------\":PRINT",
			" 40 PRINT\"IN STARTREK YOU ARE CAPTAIN OF THE STAR SHIP\"",
			" 50 PRINT\"ENTERPRISE TRAVELLING THROUGH THE GALAXY\"",
			" 60 PRINT\"HUNTING DOWN AND DESTROYING THE EVIL KLINGONS.\"",
			" 70 PRINT:PRINT\" YOU HAVE 40 'STARDATES' TO FIND AND DESTROY\"",
			" 80 PRINT\"ALL THE KLINGONS, BEFORE THEY DEVELOP THEIR\"",
			" 90 PRINT\"SECRET WEAPON AND TAKE OVER THE GALAXY,\"",
			" 100 PRINT\"ENSLAVE HUMANITY ETC. ETC.",
			" 110 PRINT:PRINT:GOSUB9000",
			" 120 PRINT:PRINT:PRINT\"YOUR SHIP IS ARMED WITH PHOTON TORPEDOES (8)\"",
			" 130 PRINT\"AND PHASARS, YOU ALSO HAVE VARIOUS SCANNERS\"",
			" 140 PRINT\"WITH WHICH TO LOCATE THE KLINGONS.\"",
			" 150 PRINT:PRINT\"YOU DRIVE YOUR SHIP BY REQUESTING\"",
			" 160 PRINT\"VARIOUS 'FUNCTIONS' WHEN YOU SEE THE '?'\"",
			" 170 PRINT\"(THE FIRST '?' IN THE PROGRAM IS ASKING FOR\"",
			" 180 PRINT\"A RANDOM NUMBER TO RE SEED THE RANDOM\"",
			" 185 PRINT\"NUMBER GENERATOR)\"",
			" 190 PRINT:PRINT\"THE FUNCTIONS ARE:-\"",
			" 210 PRINT:PRINT:PRINT:GOSUB 9000:PRINT:PRINT:PRINT:PRINT",
			" 220 PRINT:PRINT:PRINT\"S-HORT RANGE SCAN\":PRINT\"L-ONG RANGE SCAN\"",
			" 230 PRINT\"G-ALAXY SCAN\":PRINT\"M-ANOUVRE\":PRINT\"E-NGINEERING\"",
			" 240 PRINT\"P-HASARS :PRINT\"T-ORPEDOES\":PRINT\"C-OMPUTER\"",
			" 245 PRINT\"D-AMAGE REPORT\"",
			" 250 PRINT\"J-UMP DRIVE\":PRIBT",
			" 260 PRINT\"THESE ARE DETAILED AS FOLLOWS:-\":PRINT:GOSUB9000",
			" 270 PRINT:PRINT:PRINT:PRINT\"SHORT RANGE SCAN\"",
			" 275 PRINT\"----------------\":PRINT",
			" 280 PRINT\"THIS IS A DETAILED SCAN OF THE GALACTIC\"",
			" 290 PRINT\"SECTOR YOU ARE IN WHICH DISPLAYS WHAT IS\"",
			" 300 PRINT\"IN EACH LOCATION, USING THE FOLLOWING CODES:-\"",
			" 310 PRINT:PRINT\"E=ENTERPRISE (YOU ARE HERE!)\"",
			" 320 PRINT\"K=KLINGON (WHICH WILL NOW SHOOT AT YOU!)\"",
			" 330 PRINT\"B=STARBASE (WHERE YOU CAN RE-FUEL)\"",
			" 340 PRINT\"*=A STAR (THESE JUST GET IN THE WAY)\"",
			" 350 PRINTCHR$(213);\"=EMPTY SPACE\"",
			" 360 PRINT:PRINT\"(EACH SECTOR IS 4 BY 4 'WARPS')\"",
			" 370 GOSUB9000:PRINT:PRINT:PRINT:PRINT",
			" 400 PRINT\"LONG RANGE SCAN\":PRINT\"---------------\":PRINT",
			" 410 PRINT\"THIS GIVES GENERAL INFORMATION ABOUT\"",
			" 420 PRINT\"YOUR SECTOR AND THE 8 SECTORS\"",
			" 430 PRINT\"SURROUNDING IT (YOURS IS THE MIDDLE ONE)\"",
			" 440 PRINT:PRINT\"THE 'TENS' DIGIT IS THE NO. OF KLINGONS\"",
			" 450 PRINT\"AND THE 'UNITS' IS THE NUMBER OF\"",
			" 460 PRINT\"STARBASES IN EACH SECTOR\"",
			" 470 PRINT\"E.G. '21' MEANS TWO KLINGINS\"",
			" 480 PRINT\"AND ONE STARBASE IN THAT SECTOR\"",
			" 490 PRINT\"(A '9' MEANS THAT SECTOR IS OUTSIDE\"",
			" 500 PRINT\"THE GALAXY-THEREFORE PROHIBITED)\"",
			" 510 PRINT:GOSUB9000",
			" 600 PRINT\"GALAXY SCAN\":PRINT\"-----------\":PRINT",
			" 610 PRINT\"THIS IS A COMPUTERIZED RECORD OF EVERYWHERE\"",
			" 620 PRINT\"YOU HAVE BEEN TO-USING THE SAME CODE AS\"",
			" 630 PRINT\"LONG RANGE SCAN, EXCEPT THAT '**'\"",
			" 640 PRINT\"DENOTES UNEXPLORED AREAS\"",
			" 650 PRINT:PRINT\"THE GALAXY SCAN IS UPDATED BY THE\"",
			" 660 PRINT\"SHORT RANGE OR LONG RANGE SCANS\"",
			" 670 PRINT:PRINT:PRINT:GOSUB9000",
			" 700 PRINT\"MANOUVRE\":PRINT\"--------\":PRINT",
			" 710 PRINT\"TO MOVE YOUR SHIP AROUND THE GALAXY\"",
			" 720 PRINT\"YOU MAY USE YOUR WARP ENGINES\"",
			" 730 PRINT\"THIS COSTS ENERGY AT A RATE OF 20\"",
			" 740 PRINT\"ENERGY UNITS PER 'WARP'\"",
			" 750 PRINT\"(REMEMBER EACH SECTOR IS 4 WARPS ACROSS)\"",
			" 752 PRINT\"FIRST YOU INPUT YOUR COURSE IN O'CLOCK\"",
			" 754 PRINT\"I.E. 12(O'CKOCK)=UP THE SCREEN ETC.\"",
			" 760 PRINT\"THEN YOU INPUT THE WARP SPEED(1-8)\"",
			" 770 PRINT\"YOU WISH TO GO AT, THEN THE TIME(1-5)\"",
			" 780 PRINT\"IN STARDATES YOU WISH TO TRAVEL AT\"",
			" 790 PRINT\"THAT SPEED FOR\"",
			" 795 PRINT:GOSUB9000",
			" 800 PRINT:PRINT\"IF YOU ARE GOING TO HIT SOMTHING\"",
			" 810 PRINT\"THE COMPUTER WILL WARN YOU AND ASK\"",
			" 820 PRINT\"YOU TO RE-INPUT COURSE ETC.\"",
			" 830 PRINT:PRINT\"TO DOCK WITH A STARBASE AND REPLENISH\"",
			" 840 PRINT\"YOUR ENERGY AND REPAIR YOUR SHIP\"",
			" 850 PRINT\"(WHICH TAKES 5 STARDATES)\"",
			" 860 PRINT\"SIMPLY MOVE ALONGSIDE-THE COMPUTER\"",
			" 870 PRINT\"WILL ASK IF YOU WISH TO DOCK (ANSWER 'YES')\"",
			" 880 PRINT:PRINT:PRINT:GOSUB 9000",
			" 900 PRINT\"ENGINEERING\":PRINT\"-----------\":PRINT",
			" 910 PRINT\"THIS TELLS YOU HOW MUCH ENERGY YOU HAVE LEFT\"",
			" 920 PRINT\"(YOU START WITH 3000 UNITS)\"",
			" 930 PRINT\"AND HOW MUCH HAS BEEN DIVERTED TO SHIELDS\"",
			" 940 PRINT\"(YOUR SHIELDS PROTECT YOU FROM KLONGON\"",
			" 950 PRINT\"FIRE BY ABSORBING IT-UNTIL THEY RUN OUT)\"",
			" 960 PRINT\"YOU MAY THEN RESET YOUR SHIELDS\"",
			" 970 PRINT\"OR DIVERT THEIR ENERGY IF NEEDED\"",
			" 980 PRINT:PRINT:PRINT:PRINT:GOSUB 9000",
			" 1000 PRINT\"PHASERS\":PRINT\"-------\":PRINT",
			" 1010 PRINT\"THIS IS WHERE YOU GET TO SHOOT AT THEM!\"",
			" 1020 PRINT:PRINT\"SIMPLY INPUT THE AMOUNT OF ENERGY\"",
			" 1030 PRINT\"YOU ARE USING-IT WILL BE DEVIDED\"",
			" 1040 PRINT\"EQUALLY BETWEEN THE KLINGONS IN YOUR SECTOR\"",
			" 1060 PRINT\"THOSE NOT DESTROYED WILL BE DAMAGED\"",
			" 1070 PRINT:PRINT\"EACH KLONGON CAN TAKE UP TO 100\"",
			" 1080 PRINT\"ENERGY UNITS OF PHASER FIRE-BUT THIS\"",
			" 1090 PRINT\"IS DEVIDED BY THE INTERVENING DISTANCE\"",
			" 1100 PRINT\"DUE TO DISPERSION\":PRINT:GOSUB 9000",
			" 1110 PRINT\"TORPEDOES\":PRINT\"---------\":PRINT",
			" 1120 PRINT\"YOU HAVE 8 OF THESE, WHICH COST NOTHING\"",
			" 1130 PRINT\"IN ENERGY TO FIRE (BUT YOU CAN ONLY\"",
			" 1140 PRINT\"FIRE ONE AT A TIME)\"",
			" 1150 PRINT:PRINT\"EACH HIT EITHER DESTROYS THE KLINGON\"",
			" 1160 PRINT\"OR DOES 1-100 POINTS OF DAMAGE\"",
			" 1170 PRINT:PRINT:PRINT\"(LOAD NEXT PROGRAM FOR MORE \";",
			" 1180 PRINT\"INSTRUCTIONS)\":END",
			" 9000 PRINT\"(PRESS SPACE BAR TO TURN PAGE)\";",
			" 9010 POKE 530,1:POKE 57088,253",
			" 9020 WAIT 57088,255,255:PRINT:PRINT:PRINT:RETURN",
			"OK",
		};
	}
}
