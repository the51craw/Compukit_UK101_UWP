namespace Compukit_UK101_UWP
{
	public partial class BasicProg 
	{
		public string[] SolvingSimultaneousEquations = new string[] {
			"",
			"",
			" 10 PRINT:PRINT:PRINT\"SOLVING SIMULTANEOUS EQUATIONS!!!\"",
			" 20 PRINT\"---------------------------------\":PRINT",
			" 25 GOTO6000",
			" 30 DIM A(10,11):INPUT\"NO. OF EQUATIONS\";N2",
			" 40 N4=N2+1:FOR I=1 TO N2:FOR J=1 TO N4",
			" 50 PRINT\"A(\"I\",\"J\")=\";:INPUT A(I,J)",
			" 60 NEXT J:NEXT I",
			" 5100 FOR K=1 TO N2:P=A(K,K):A(K,K)=1",
			" 5110 IF P=0 THENPRINT\"ZERO DIAG ELEMENT\":END",
			" 5120 FOR J=K+1TON4:A(K,J)=A(K,J)/P:NEXT J:I=1",
			" 5130 IF I=K THEN 5200",
			" 5140 R=A(I,K):FOR J=1 TO N4",
			" 5150 A(I,J)=A(I,J)-R*A(K,J):NEXT J",
			" 5160 A(I,K)=0",
			" 5200 I=I+1:IF I=N4 THEN 5250",
			" 5210 GOTO 5130",
			" 5250 NEXT K:PRINT\"THE ROOTS ARE:-\"",
			" 5260 FOR I=1 TO N2:PRINT\"X(\"I\")=\"A(I,N4)",
			" 5270 NEXT I:END",
			" 6000 PRINT\"THIS PROGRAM SOLVES THE SET OF N EQUATIONS:\"",
			" 6010 PRINT\"A(1,1)*X(1)+...+A(1,N)*X(N)=A(1,N+1)\"",
			" 6020 PRINT\"A(2,1)*X(1)+...+A(2,N)*X(N)=A(2,N+1)\"",
			" 6030 PRINT\".\":PRINT\".\":PRINT\".\"",
			" 6040 PRINT\"A(N,1)*X(1)+...+A(N,N)*X(N)=A(N,N+1)\"",
			" 6050 PRINT:PRINT\"YOU SIMPLY STATE THE NO. OF EQUATIONS\"",
			" 6060 PRINT\"(THE VALUE OF N ABOVE) AND THE VALUES\"",
			" 6070 PRINT\"OF THE CONSTANTS A(1,1) ETC.\"",
			" 6080 PRINT\"AND THE COMPUTER CALCULATES THE UNKNOWNS\"",
			" 6090 GOTO30",
			"OK",
		};
	}
}
