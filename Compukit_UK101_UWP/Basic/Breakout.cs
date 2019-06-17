namespace Compukit_UK101_UWP
{
	public partial class BasicProg 
	{
		public string[] Breakout = new string[] {
			"",
			"",
			" 5 DIMS(3):GOSUB7000",
			" 10 GOSUB5000:GOSUB90",
			" 20 IFH=32THENPOKEP,32",
			" 30 IFH<>32THENGOSUB110",
			" 40 BX=BX+VX*S:BY=BY+VY*S/2:P=SC+INT(BX+.5)+INT(BY+.5)*64",
			" 50 H=PEEK(P):IFH=32THENPOKEP,226",
			" 60 POKEK,254:K1=PEEK(K):IFK1<254THENGOSUB90",
			" 70 IFBY<17THEN20",
			" 80 GOTO2000",
			" 90 POKEBT+B-2,32:POKEBT+B+2,32",
			" 95 IF(K1AND2)=0ANDB<47THENB=B+1",
			" 100 IF(K1AND4)=0ANDB>1THENB=B-1",
			" 102 POKEBT+B-1,145:POKEBT+B,145:POKEBT+B+1,145",
			" 104 POKEBT+B-2,145:POKEBT+B+2,145",
			" 106 RETURN",
			" 110 IF(BX<2ORBX>46)ANDSGN(BX-30)=SGN(VX)THENVX=-VX:BH=0:GOTO1000",
			" 120 IFBY<2THENVY=ABS(VY):BH=0:GOTO1000",
			" 125 IFH<>145THEN130",
			" 127 BH=0:D=INT(BX+.5)-B:VX=SGN(D)*1.5:VY=(D=0)*1.5-1.5",
			" 130 IFH<>161ANDH<>187ORBH=1THENRETURN",
			" 140 BH=1:POKEP,32:SS=SS+(10-INT(BY+.5))*5:VY=-VY",
			" 142 IFSS>2060THEN10",
			" 145 IFS<S(INT(BY+.5)-5)THENS=S(INT(BY+.5)-5)",
			" 147 GOTO1000",
			" 1000 R=SGN(RND(1)-.5)*.5",
			" 1005 VX=VX+R*SGN(VX+RND(1)/10-.05):VY=VY-R*SGN(VY+RND(1)/10-.05)",
			" 1010 IFVX=0THENVX=SGN(RND(1)-.5)/2:VY=VY-SGN(VX)/2",
			" 1015 IFVY=0THENVY=SGN(RND(1)-.5)/2:VX=VX-SGN(VX)/2",
			" 1020 RETURN",
			" 2000 POKESC+INT(BX)+INT(BY)*64,32",
			" 2010 S=S(3):GOSUB5090",
			" 2020 FORI=1TO48:POKESC+1024+I,32:NEXT",
			" 2030 NB=NB-1:IFNB>0THENGOSUB90:GOTO20",
			" 2040 FORI=1TO16:PRINT:NEXT",
			" 2050 PRINT\"END OF GAME\"",
			" 2060 PRINT\"YOUR SCORE:-\";SS",
			" 2070 END",
			" 5000 NB=5",
			" 5005 Z=(10-DF):PRINTCHR$(12);",
			" 5006 BT=54220",
			" 5010 SC=53196:K=57088",
			" 5020 FORI=1TO48:POKE(SC+64+I),150:NEXT",
			" 5030 FORI=1TO16:POKE(SC+1+64*I),157",
			" 5040 POKESC+48+I*64,156:NEXT",
			" 5050 FORI=6TO8STEP2:FORJ=2TO47",
			" 5060 POKESC+I*64+J,187:NEXTJ,I",
			" 5070 FORJ=2TO47",
			" 5080 POKESC+448+J,161:NEXTJ",
			" 5090 B=25:BX=25:BY=10:VX=1.5:VY=1.5",
			" 5095 S(1)=1/3:S(2)=1/4.5:S(3)=1/6",
			" 5100 S=S(3):P=SC+BX+BY*64",
			" 5500 RETURN",
			" 7000 PRINTCHR$(12):PRINT:PRINT\" BREAKOUT!\":PRINT\" ---------\":PRINT",
			" 7010 PRINT\" YOU HAVE TO BREAK THROUGH THE WALL AND SCORE\"",
			" 7020 PRINT\"AS MANY POINTS AS YOU CAN BY MOVING YOUR\"",
			" 7030 PRINT\"BAT (USING SHIFT KEYS) TO HIT THE BALL\"",
			" 7040 PRINT\"INTO THE WALL.\"",
			" 7050 PRINT\"YOU SCORE: 10 POINTS FOR EACH FIRST LAYOR BRICK\"",
			" 7060 PRINT\"           15 POINTS FOR SECOND LAYOR\"",
			" 7070 PRINT\"       AND 20 POINTS FOR THIRD LAYOR\"",
			" 7080 PRINT:PRINT\"PRESS SPACE TO START\";:POKE530,1",
			" 7090 POKE57088,253",
			" 7100 IFPEEK(57088)=255THEN7100",
			" 7110 GOTO10",
			"OK",
		};
	}
}
