namespace Compukit_UK101_UWP
{
	public partial class BasicProg 
	{
		public string[] Hexapawn = new string[] {
			"",
			"",
			" 10 FORI=1TO16:PRINT:NEXT:PRINT\"HEXAPAWN!!!\"",
			" 20 PRINT\"-----------\":PRINT:PRINT",
			" 30 INPUT\"INSTRUCTIONS\";Q$:IFLEFT$(Q$,1)=\"N\"THEN1000",
			" 40 PRINT:PRINT:PRINT\"THIS IS HEXAPAWN. THE BOARD IS A 3 BY 3\"",
			" 50 PRINT\"CHESS BOARD WITH THREE PAWNS IN EACH SIDE.\"",
			" 60 PRINT\"THE PAWNS MOVE AS IN CHESS.\"",
			" 70 PRINT\"TO WIN YOU MUST EITHER GET A PAWN TO THE\"",
			" 80 PRINT\"OPPOSITE SIDE, OR TAKE ALL THE COMPUTERS\"",
			" 90 PRINT\"PAWNS, OR GET THE COMPUTER IN SUCH A POSITION\"",
			" 100 PRINT\"THAT I CANNOT MOVE.\"",
			" 110 PRINT\"TO MOVE, PRESS THE KEY OF THE PIECE\"",
			" 120 PRINT\"FOLLOWED BY THE KEY OF ITS DESTINATION\"",
			" 130 PRINT\"THE KEYS TO USE ARE:   Q  W  E\"",
			" 140 PRINT\"                       A  S  D\"",
			" 150 PRINT\"                       Z  X  C\"",
			" 160 PRINT\"YOU ARE WHITE AND MOVE FIRST, SO",
			" 170 PRINT\"YOUR PAWNS ARE AT  'Z', 'X' AND 'C'.\"",
			" 180 PRINT\"PRESS SPACE BAR TO START GAME\";",
			" 190 POKE 530,1:POKE 57088,253:WAIT57088,255,255",
			" 1000 SC=53196:BD=53392:DL=54220",
			" 1001 PRINT:PRINT\"I CAN USE RANDOM OR SEQUENTIAL MOVE SELECTION\"",
			" 1002 INPUT\"WHICH DO YOU WANT (R OR S)\";R$",
			" 1005 FORI=1TO16:PRINT:NEXT",
			" 1006 PRINT\"                                MY WINS:\"",
			" 1007 PRINT\"                              YOUR WINS:\"",
			" 1008 PRINT:PRINT:PRINT:PRINT:PRINT:PRINT",
			" 1010 DIMMC(2,2),BD(3,4),BM(3,3,12),M(3),P(2,3)",
			" 1020 CP=53749:HP=53813:HG=0:CW=0",
			" 1025 POKE 11,0:POKE 12,253",
			" 1030 DATA 81,87,69,65,83,68,90,88,67",
			" 1032 Q$=STR$(HW):FORI=1TOLEN(Q$)",
			" 1033 POKE(HP+I),ASC(MID$(Q$,I,1)):NEXT",
			" 1034 Q$=STR$(CW):FORI=1TOLEN(Q$)",
			" 1035 POKE(CP+I),ASC(MID$(Q$,I,1)):NEXT",
			" 1036 RESTORE",
			" 1040 FORI=0TO2:FORJ=0TO2:READMC(I,J):NEXTJ,I",
			" 1050 FORI=2TO29:POKE(SC+I+128),161",
			" 1060 POKE(SC+I+960),161:NEXT",
			" 1070 FORI=130 TO 898 STEP 64:POKE(SC+I),161",
			" 1080 POKE(SC+I+1),161:POKE(SC+I+26),161",
			" 1090 POKE(SC+I+27),161:NEXT",
			" 1100 FORX=1TO3:FORY=1TO3:BD(X,Y)=X-2:NEXTY,X",
			" 1200 GOSUB 2000",
			" 1210 GOTO 5000",
			" 2000 REM BD TO SCRN",
			" 2010 FORX=1TO3:FORY=1TO3",
			" 2020 CO=32:IF(ABS(X/2-INT(X/2))=ABS(Y/2-INT(Y/2)))THENCO=187",
			" 2030 PS=BD+(X-1)*256+(Y-1)*8",
			" 2035 IFPEEK(PS)=COANDPEEK(PS+67)<>COANDBD(X,Y)<>0THEN2060",
			" 2040 FORI=0TO3:FORJ=0TO7",
			" 2050 POKE(PS+I*64+J),CO:NEXT J,I",
			" 2060 IF BD(X,Y)=0 THEN 2500",
			" 2070 IF BD(X,Y)=1 THEN 2290",
			" 2075 IF PEEK(PS+67)=227 THEN 2500",
			" 2080 POKE(PS+67),227:POKE(PS+68),228",
			" 2090 POKE(PS+130),176:POKE(PS+131),6",
			" 2100 POKE(PS+132),6:POKE(PS+133),178:GOTO2500",
			" 2290 IFPEEK(PS+67)=234 THEN 2500",
			" 2300 POKE(PS+67),234:POKE(PS+68),235",
			" 2310 POKE(PS+130),176:POKE(PS+131),161",
			" 2320 POKE(PS+132),161:POKE(PS+133),178",
			" 2500 NEXT Y,X:RETURN",
			" 3000 REM INPUT H GO",
			" 3005 M$=\"YOUR MOVE                        \":GOSUB 4000",
			" 3010 GOSUB 3500:XI=X+1:YI=Y+1",
			" 3020 GOSUB 3500:XF=X+1:YF=Y+1",
			" 3030 IF BD(XI,YI)<>1 OR ABS(YF-YI)>1 THEN 3200",
			" 3035 IF XI-XF<>1 THEN 3200",
			" 3040 IF ABS(YF-YI)=1 AND BD(XF,YF)<>-1THEN3200",
			" 3050 IF YI=YF AND BD(XF,YF)<>0 THEN 3200",
			" 3060 BD(XI,YI)=0:BD(XF,YF)=1:GOSUB 2000",
			" 3070 RETURN",
			" 3200 M$=\"ILLEGAL MOVE!\":GOSUB 4000:GOTO3000",
			" 3500 REM INPUT A CODE",
			" 3510 X=USR(0):G=PEEK(531)",
			" 3520 FORX=0TO2:FORY=0TO2:IFG=MC(X,Y)THENRETURN",
			" 3530 NEXT Y,X:M$=\"NOT IN THIS GAME!\"",
			" 3540 GOSUB 4000:M$=\"                   \"",
			" 3550 GOSUB 4000:GOTO 3500",
			" 4000 REM DISPLAY MESSAGE M$",
			" 4010 FORI=1TOLEN(M$)",
			" 4020 POKE(DL+I),ASC(MID$(M$,I,1)):NEXT",
			" 4025 IFLEFT$(M$,4)=\"YOUR\"THEN RETURN",
			" 4030 FORI=1TO1000:NEXT:RETURN",
			" 5000 REM START GAME",
			" 5010 GOSUB 3000",
			" 5020 FORI=1TO3:IF BD(1,I)=1 THEN 8000",
			" 5040 NEXTI:IFR$=\"R\"THEN7000",
			" 5045 XI=2:YI=1",
			" 5050 IF BD(XI,YI)<>-1 THEN 6010",
			" 5060 FOR W=-1 TO 1",
			" 5070 IF BD(XI+1,YI+W)<>ABS(W) THEN 6000",
			" 5080 PT=BD(XI+1,YI+W):BD(XI,YI)=0",
			" 5085 BD(XI+1,YI+W)=-1",
			" 5090 FORL=0TOHW:FORI=1TO3:FORJ=1TO3",
			" 5100 IF BM(I,J,L)<>BD(I,J)THEN 5115",
			" 5110 NEXT J,I:GOTO 5200",
			" 5115 NEXT L",
			" 5120 GOSUB 2000:FORI=1TO3:FORJ=1TO3",
			" 5130 BM(I,J,HW)=BD(I,J):NEXT J,I",
			" 5140 FORI=1TO3:IFBD(3,I)=-1THEN9000",
			" 5150 NEXT I:FORXT=2TO3:FORYT=1TO3",
			" 5160 IF BD(XT,YT)<>1 THEN 5190",
			" 5170 FORW1=-1TO1:IFBD(XT-1,YT+W1)=-ABS(W1)THEN5000",
			" 5180 NEXT W1",
			" 5190 NEXT YT,XT:GOTO 9000",
			" 5200 BD(XI,YI)=-1:BD(XI+1,YI+W)=PT",
			" 6000 NEXT W",
			" 6010 YI=YI+1:IFYI<4THEN5050",
			" 6012 YI=1:XI=XI-1:IFXI<1THEN5050",
			" 7000 REM RANDOM MOVE SELECTOR",
			" 7010 FORX=1TO2:FORY=1TO3:P(X,Y)=0:NEXTY,X:N1=0",
			" 7020 N2=0:XI=INT(RND(1)*2)+1:YI=INT(RND(1)*3)+1",
			" 7030 IFP(XI,YI)=1 THEN 7020",
			" 7040 IF BD(XI,YI)<>-1 THEN 7500",
			" 7050 FORW=1TO3:M(W)=0:NEXT W",
			" 7060 W=INT(RND(1)*3)+1:IFM(W)=1THEN7060",
			" 7070 IF BD(XI+1,YI-2+W)<>ABS(2-W)THEN 7490",
			" 7080 PT=BD(XI+1,YI-2+W):BD(XI,YI)=0",
			" 7090 BD(XI+1,YI-2+W)=-1",
			" 7100 L=0",
			" 7105 FORI=1TO3:FORJ=1TO3",
			" 7110 IF BD(I,J)<>BM(I,J,L)THEN7130",
			" 7120 NEXT J,I:BD(XI+1,YI-2+W)=PT:BD(XI,YI)=-1:GOTO 7490",
			" 7130 L=L+1:IF L<HW THEN 7105",
			" 7135 GOSUB 2000",
			" 7140 FORI=1TO3:FORJ=1TO3:BM(I,J,HW)=BD(I,J)",
			" 7150 NEXT J,I",
			" 7155 IFXI=2THEN9000",
			" 7160 FORXT=2TO3:FORYT=1TO3:IFBD(XT,YT)<>1THEN7200",
			" 7170 FORWT=-1TO1:IFBD(XT-1,YT+WT)=-ABS(WT)THEN5000",
			" 7180 NEXT WT",
			" 7200 NEXT YT,XT:GOTO 9000",
			" 7490 M(W)=1:N2=N2+1:IFN2<3THEN7060",
			" 7500 P(XI,YI)=1:N1=N1+1:IFN1<6THEN 7020",
			" 7510 GOTO 8000",
			" 8000 M$=\"YOU WIN!  ANOTHER GAME?\":GOSUB 4000",
			" 8005 HW=HW+1",
			" 8010 X=USR(0):IFPEEK(531)=89 THEN 1030",
			" 8020 FORI=1TO16:PRINT:NEXT:PRINT\"SOUR GRAPES!\"",
			" 8030 PRINT:PRINT\"COMPUTER WINS:\";CW",
			" 8040 PRINT\"HUMAN WINS:\";HW:PRINT:PRINT:END",
			" 9000 M$=\"I WIN!     ANOTHER GAME?\"",
			" 9010 CW=CW+1:GOSUB 4000:GOTO 8010",
			"OK",
		};
	}
}
