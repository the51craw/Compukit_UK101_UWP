namespace Compukit_UK101_UWP
{
	public partial class BasicProg 
	{
		public string[] DrawPictures = new string[] {
			"",
			"",
			" 1 DATA160,0,169,32,153,0,208,153,0,209,153,0",
			" 2 DATA210,153,0,211,153,0,212,153,0,213,153,0",
			" 3 DATA214,153,0,215,200,208,229,96",
			" 4 FORI=576TO607:READA:POKEI,A:NEXT",
			" 9 POKE 11,0:POKE 12,253",
			" 10 REM THE LINES ABOVE ARE A",
			" 11 REM FAST CLEAR SCREEN ROUTINE",
			" 12 REM TO USE IT PUT: POKE 11,64:POKE 12,2",
			" 13 REM THEN X=USR(0) WILL CLEAR THE SCREEN",
			" 14 REM USE POKE 579,N TO FILL SCREEN",
			" 15 REM WITH CHR$(N)",
			" 16 REM RUN 2000 FOR AN EXAMPLE",
			" 19 GOTO1000",
			" 20 PRINT\"     1-LEFT   2-RIGHT   3-UP    4-DOWN\":FORI=1TO14:PRINT:NEXT",
			" 30 X=1:Y=16:SC=53196:POKE(SC+X+Y*64),32",
			" 31 SC=SC-13",
			" 40 L=32:X=23:Y=8:POKE(SC+X+Y*64),43",
			" 50 PP=USR(0):A=PEEK(531)",
			" 55 IF A=32 THEN POKE(SC+Y*64+X),32:GOTO 50",
			" 60 IF A>64 THEN 200",
			" 65 IF PEEK(SC+X+Y*64)=43THENPOKE(SC+X+Y*64),L",
			" 70 A=A-48:IF A=1 THEN X=X-1",
			" 80 IF A=2 THEN X=X+1",
			" 90 IF A=3 THEN Y=Y-1",
			" 100 IF A=4 THEN Y=Y+1",
			" 110 L=PEEK(SC+X+Y*64):POKE(SC+X+Y*64),43",
			" 120 GOTO 50",
			" 200 PP=USR(0):A1=PEEK(531)",
			" 210 N=(A-65)*10+A1-48:IF N<0 OR N>255 THEN 50",
			" 220 POKE(SC+Y*64+X),N:GOTO 50",
			" 1000 PRINTCHR$(26):PRINT:PRINT\"GRAPHICS\":PRINT\"--------\":PRINT",
			" 1010 PRINT\"THIS PROGRAM ALLOWS YOU TO DRAW PICTURES\"",
			" 1020 PRINT\"ON THE SCREEN BY USING COMPUKITS GRAPHICS\"",
			" 1030 PRINT\"THE '+' IS YOUR CURSER, YOU MOVE IT\"",
			" 1040 PRINT\"WITH KEYS 1-4, AS INDICATED.\"",
			" 1050 PRINT\"TO PLOT A CHARACTER PRESS A LETTOR\"",
			" 1060 PRINT\"FOLLOWED BY A DIGIT, THE LETTOR GIVES\"",
			" 1070 PRINT\"THE FIRST TWO DIGITS(A=0, B=10, C=20, ETC.)\"",
			" 1080 PRINT\"AND THE NUMBER GIVES THE LAST DIGIT OF\"",
			" 1090 PRINT\"THE CHARACTER PLOTTED. EG 'Q1'=161\"",
			" 1100 PRINT\"SPACE WILL CLEAR THE CHARACTER\"",
			" 1105 POKE 530,1:POKE 57088,2",
			" 1110 PRINT:PRINT\"PRESS SPACE TO START\";",
			" 1111 IFPEEK(57088)=0THEN1111",
			" 1120 POKE530,0:POKE11,64:POKE12,2:X=USR(0)",
			" 1125 PRINT",
			" 1130 POKE11,0:POKE12,253:GOTO20",
			" 2000 POKE 11,64:POKE 12,2",
			" 2010 FORI=0TO255:POKE579,I:X=USR(0)",
			" 2020 FORX=1TO10:NEXTX,I:POKE579,32",
			" 2030 X=USR(0):PRINT\"   HOWS THAT FOR FAST GRAPHICS!\"",
			" 2040 PRINT:PRINT:PRINT:PRINT:END",
			"OK",
		};
	}
}
