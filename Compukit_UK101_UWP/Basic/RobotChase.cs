namespace Compukit_UK101_UWP
{
	public partial class BasicProg
	{
		public string[] RobotChase = new string[] {
			"",
			"",
			" 1 POKE11,0:POKE12,253",
			" 10 PRINTCHR$(26):PRINT:PRINT",
			" 30 PRINT\"     ROBOT CHASE!!!\"",
			" 40 PRINT\"     --------------\"",
			" 50 PRINT:DIM R(6,2):SC=53196",
			" 55 SC=SC-13",
			" 60 PRINT:PRINT\"THE OBJECT OF THE GAME IS FOR YOU ()\"",
			" 70 PRINT\"TO DESTROY ALL THE ROBOTS (#) ON THE MINES (+)\"",
			" 80 PRINT\"WITHOUT LETTING THEM CATCH YOU OR GETTING\"",
			" 90 PRINT\"ELECTRICUTED ON THE MINES YOURSELF.\"",
			" 100 PRINT\"THE ROBOTS ALWAYS MOVE DIRECTLY TOWARDS YOU\"",
			" 110 PRINT\"WHENEVER YOU MOVE.\"",
			" 120 PRINT:PRINT\"TO MOVE USE THE KEYS 'AROUND' KEY 'S'\"",
			" 125 PRINT",
			" 130 PRINT\"Q=UP AND LEFT,   W=UP,       E=UP AND RIGHT\"",
			" 140 PRINT\"A=LEFT,          S=STAY PUT, D=RIGHT\"",
			" 150 PRINT\"Z=DOWN AND LEFT, X=DOWN,     C=DOWN AND RIGHT\"",
			" 160 PRINT",
			" 200 PRINT",
			" 210 INPUT\"DIFFICULTY (1=EASY TO 10=IMPOSSIBLE)\";D",
			" 230 D=INT(ABS(D)):S=1:N=6:M=100-(D*10)",
			" 240 IF D>10 THEN 210",
			" 250 GOSUB2000",
			" 280 FOR I=1 TO M+7",
			" 290 X=INT(RND(123.424)*48)+1",
			" 300 Y=INT(RND(1)*31)+1",
			" 320 IF PEEK(SC+64*Y+X)<>32 THEN 290",
			" 325 IF I>M THEN 350",
			" 330 POKE(SC+64*Y+X),ASC(\"+\")",
			" 340 NEXT I",
			" 350 IF I=M+7 THEN 400",
			" 360 R(I-M,1)=X:R(I-M,2)=Y",
			" 370 POKE(SC+Y*64+X),ASC(\"#\")",
			" 380 NEXT I",
			" 400 PX=X:PY=Y:POKE(SC+Y*64+X),29",
			" 410 NEXT I",
			" 590 REM ALL PRINTED OUT",
			" 600 N$=STR$(N)+\"-ROBOTS LEFT    \"",
			" 602 FORI=1TOLEN(N$):P=ASC(MID$(N$,I,1))",
			" 604 POKE55247+I,P:NEXT",
			" 610 OO=USR(0):G=PEEK(531)",
			" 630 POKE(SC+PY*64+PX),32",
			" 640 IF G=81 OR G=87 OR G=69 THEN PY=PY-1",
			" 650 IF G=90 OR G=88 OR G=67 THEN PY=PY+1",
			" 660 IF G=81 OR G=65 OR G=90 THEN PX=PX-1",
			" 670 IF G=69 OR G=68 OR G=67 THEN PX=PX+1",
			" 700 T=PEEK(SC+PY*64+PX)",
			" 720 IF T<>ASC(\"+\") THEN 750",
			" 730 GOSUB 2000:PRINT\"YOU HIT A MINE\":PRINT",
			" 735 PRINT\"YOU'VE BEEN ELECTRICUTED!\"",
			" 740 GOTO 870",
			" 750 IF T<>ASC(\"#\") THEN 810",
			" 790 GOSUB2000:PRINT\"YOU WALKED INTO A ROBOT!!!\"",
			" 800 GOTO 870",
			" 810 IFPX<1ORPX>48ORPY<1ORPY>31THEN860",
			" 850 GOTO 890",
			" 860 GOSUB 2000:PRINT\"YOU LEFT THE SCREEN!\"",
			" 862 PRINT:PRINT\"THATS CHEATING!\"",
			" 870 PRINT:PRINT\"I WIN!\"",
			" 880 PRINT:INPUT\"ANOTHER GAME\";Q$:IF LEFT$(Q$,1)=\"Y\"THEN200",
			" 885 PRINT:PRINT\"THANKYOU\":END",
			" 890 POKE(SC+PY*64+PX),29",
			" 893 PRINTCHR$(13);",
			" 895 REM PERSON MOVED-NOW ROBOTS",
			" 900 FOR I=1 TO 6",
			" 905 IF R(I,1)=0 THEN 1050",
			" 910 X1=R(I,1):Y1=R(I,2)",
			" 920 POKE(SC+Y1*64+X1),32",
			" 930 IF X1-PX>0 THEN X1=X1-1",
			" 940 IF PX-X1>0 THEN X1=X1+1",
			" 950 IF Y1-PY>0 THEN Y1=Y1-1",
			" 960 IF PY-Y1>0 THEN Y1=Y1+1",
			" 980 IF PEEK(SC+Y1*64+X1)<>ASC(\"+\") THEN 1030",
			" 1000 R(I,1)=0",
			" 1010 POKE(SC+Y1*64+X1),32",
			" 1020 GOTO 1050",
			" 1030 POKE(SC+Y1*64+X1),ASC(\"#\")",
			" 1040 R(I,1)=X1:R(I,2)=Y1",
			" 1050 NEXT I",
			" 1060 REM ALL ROBOTS MOVED",
			" 1070 N=0:FOR I=1 TO 6:IF R(I,1)>0 THEN N=N+1",
			" 1100 IF R(I,1)=PX AND R(I,2)=PY THEN 1140",
			" 1120 NEXT I",
			" 1130 IF N=0 THEN 1160",
			" 1135 GOTO 1180",
			" 1140 GOSUB2000:PRINT\"OH DEAR\":PRINT:PRINT\"YOU'VE BEEN CAUGHT!\"",
			" 1145 PRINT:PRINT\"FLOWERS?\":PRINT:PRINT:GOTO880",
			" 1150 GOTO 880",
			" 1160 GOSUB2000:PRINT\"ALL ROBOTS DESTROYED!\"",
			" 1165 PRINT:PRINT\"YOU WIN!\":GOTO 880",
			" 1180 GOTO 600",
			" 2000 PRINTCHR$(26);:POKE53261,32:RETURN",
            "RUN",
            " ",
		};
	}
}
