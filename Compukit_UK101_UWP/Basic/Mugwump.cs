namespace Compukit_UK101_UWP
{
	public partial class BasicProg 
	{
		public string[] Mugwump = new string[] {
			"",
			"",
			" 10 FORI=1TO16:PRINT:NEXT:PRINT\" MUGWUMP!!!\"",
			" 20 PRINT\" ----------\":PRINT",
			" 30 PRINT\"IN THIS GAME THERE ARE FOUR 'MUGWUMPS'\"",
			" 40 PRINT\"HIDDEN IN A 10 BY 10 GRID OF HOLES.\"",
			" 50 PRINT\"YOU MUST TRY TO FIND THEM BY GUESSING A\"",
			" 60 PRINT\"HOLE - THEN THE DISTANCE TO EACH MUGWUMP\"",
			" 70 PRINT\"WILL BE GIVEN.\"",
			" 80 PRINT\"YOU INPUT YOUR GUESSES AS TWO NUMBERS\"",
			" 90 PRINT\"A HORIZONTAL AND A VERTICAL COORDINATE\"",
			" 100 PRINT:PRINT:GOSUB9000",
			" 110 PRINT\"THE GRID LOOKS LIKE THIS:-\"",
			" 120 PRINT\"   0 1 2 3 4 5 6 7 8 9\"",
			" 130 A$=CHR$(213)+\" \":B$=\"\":FORI=1TO10:B$=B$+A$:NEXT",
			" 140 FORI=0TO9:PRINTI;B$:NEXT",
			" 150 POKE53918,42",
			" 160 PRINT\"SO THE * IS AT 7,5\"",
			" 170 PRINT:GOSUB9000",
			" 200 DIMA(4,2),B(4)",
			" 205 FORI=1TO4:B(I)=0:NEXT",
			" 207 PRINT\"FOUR MUGWUMPS ARE NOW HIDING\"",
			" 208 G=1",
			" 210 FORI=1TO4:FORJ=1TO2:A(I,J)=INT(RND(1)*10):NEXTJ,I",
			" 220 PRINT\"TURN NO.\";G;:INPUT\"YOUR GUESS\";A,B",
			" 230 A=INT(A):B=INT(B)",
			" 240 IFA<0ORA>9ORB<0ORB>9THENPRINT\"OUT OF RANGE\":GOTO220",
			" 242 FORI=1TO4:IFA(I,1)<>AORA(I,2)<>BTHEN248",
			" 244 PRINT\"MUGWUMP\";I;\"FOUND!\":B(I)=1",
			" 248 NEXT",
			" 250 PRINT\"MUGWUMP:   1     2     3     4\":PRINT\"DISTANCE:\";",
			" 260 FORI=1TO4",
			" 270 D=SQR((A(I,1)-A)^2+(A(I,2)-B)^2)",
			" 280 PRINTTAB(4+I*6)INT(D*10+.5)/10;:NEXT:PRINT",
			" 290 IFB(1)=1ANDB(2)=1ANDB(3)=1ANDB(4)=1THEN500",
			" 300 G=G+1:IFG<10THEN220",
			" 400 PRINT:PRINT\"THATS TEN GOES-YOU DIDNT GET THEM ALL!!\"",
			" 410 GOTO600",
			" 500 PRINT:PRINT\"WELL DONE, YOU FOUND ALL THE MUGWUMPS\"",
			" 510 PRINT\"AFTER ONLY\";G;\"GOES!\"",
			" 600 PRINT:PRINT\"THAT WAS FUN, LETS PLAY AGAIN\"",
			" 620 PRINT:PRINT:GOTO207",
			" 9000 PRINT\"PRESS SPACE TO CONTINUE\";:POKE530,1:POKE57088,253",
			" 9010 R=RND(1):IFPEEK(57088)=255THEN9010",
			" 9020 PRINT:PRINT:PRINT:RETURN",
			"OK",
		};
	}
}
