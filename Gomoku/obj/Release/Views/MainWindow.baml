   M S B A M L   `   `   `  ���� 
  Gomoku    Gomoku.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$  Gomoku��$	��693��$
��1031�� � Icon$ Images/Icon.jpeg��
���  ��$��16(� � WindowStartupLocation$ CenterScreen=�.��� $��0,0,2,7q�.+�(� $��10,10,375,10q�$
��Auto��$
��Auto��.���� 
���   �$��3q�
�������$	��460��$	��460��._�A� -    A�Name$ ugrid�� A� Columns$ 12�� A� Rows$ 12��
�������$��	1,-1,0,-1q�� $��Right=�$	��575��$��	0,14,10,0q�$	��Top=�$	��327�� � RowDefinitions �� $
��192* ��� $
��263* �.+��� -   $��btnMain��$��Start!��$
��Left=�$��	79,54,0,0q�$	��Top=�$��88��$��30�����	#FF5D9BF5
�������$��14(��� -    ��Name$ txtName��$��Guest��$
��Left=�$��30��$��	125,7,0,0q� �� TextWrapping$
 Wrap=����	#FF5D9BF5$	��Top=�$	��162��$��16(�	 �� TextAlignment$	 Center=��� $��
Your name:��$
��Left=�$��36,7,0,0q�$	��Top=�$��14(�$
��Bold"��� -   $��Lview��$��
0,118,0,70q�$	��1.5q����	#FF3564F1$��14(�$��2���� $
��Left=�$��0,92,0,0q�
�������$�� |||||  Chat/History��$	��Top=�$��26��$	��327�����	#FF157FC9$��14(��� ���	#FF157FC9$	��1.5q�$
��Left=�$��70��$��	0,262,0,0q�$	��Top=�$	��327��$��1��._�� ���	#FFDADADA$��1,-2,2,0q�$��66��.+��� -   $��txtMsg��$
��Left=�$��34��$��	19,17,0,0q�$
 Wrap=�$	��Top=�$	��161���� -   $��btnSend��$
��Send��
�������$
��Left=�$��
180,17,0,0q�$	��Top=�$��51��$��34�����	#FFF7AA69�� -   $��btnPvP��$
��PvsP��$
��Left=�$��0,351,0,-44q�$��1��$	��Top=�$��80��$��25�����	#FF889DF9�� -	   $��btnPvC��$��PvsCOM��$
��Left=�$��125,351,0,-44q�$��1��$	��Top=�$��80��$��25�����	#FF889DF9�� -
   
 ��Name$
 btnComOnline��$��	ComOnline��$
��Left=�$��
234,53,0,0q�$	��Top=�$��88��$��30�����	#FF5D9BF5
��������� $
��COM:��$
��Left=�$��
183,54,0,0q�$	��Top=�$��14(�$
��Bold"��� $��PLAYER:��$
��Left=�$��	10,55,0,0q�$	��Top=�$��14(�$
��Bold"��� -   $
 btnExit��$
��EXIT��$
��Left=�$��242,351,0,-44q�$��1��$	��Top=�$��80��$��25�����	#FF889DF9