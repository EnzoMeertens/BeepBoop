   M S B A M L   `   `   `  ����C  ?BeepBoop, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:BeepBoopBeepBoop  5   	       BeepBoop.EditWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:BeepBoop     5   	     � Title$
  Edit��5   	   $	��225��6   $	��400��6#   $	��225��6/   $	��400��6?   $	��225��6N   $	��600��6^   ���	#FF424242   	Fira Sans#��[  6�    � Icon$# Resources/Images/BeepBoop.ico��6�   .��5
      � $��5q�6    � RowDefinitions 5   
   �� 5      $��2* �6   6*   �� 5      $��2* �6   6*   �� 5      $��2* �6   6*   �� 5      $��* �6   6)   .+�5   
   �� $��0��6   $��Stretch=�6"   $��Stretch=�6>   $	��2,5q�6\   .+�5      �� 
�������$��0q�6'   $	��5,2q�63   $��One-shot��6@   6U   ]� 5      $��Stretch=�6   .+�5      �� -   5        $��Right=�5      $��6,0,2,0q�60   $	��8,0q�6A   ��6O   Z� ��5      �� 5       �� ImageSource$% Resources/Images/cross-icon.png��6%    j� Stretch$ Uniform=�6S   5      �� 5      -   5        
 � Tag$ Audio��5      $��2q�6x   $
��Left=�6�   ��6&   �� *Playback_Item_Temp.One_Shot+ �� UpdateSourceTrigger$ PropertyChanged=�6�   5      5      �� 5   
   $��1��6   $��Stretch=�6"   $��Stretch=�6>   $	��2,5q�6\   .+�5      �� 
�������$��0q�6'   $	��5,2q�63   $
��Loop��6@   6Q   ]� 5       $��Stretch=�6   .+�5!      �� -   5        $��Right=�5!      $��6,0,2,0q�60   $	��8,0q�6A   ��6O   Z� ��5"      �� 5#      $% Resources/Images/cross-icon.png��6%   $ Uniform=�6S   5%      �� 5&      -   5        $ Audio��5&      $��2q�6t   $
��Left=�6�   ��6&   �� *Playback_Item_Temp.Loop+$ PropertyChanged=�6�   5'      5(      �� 5*   
   $��2��6   $��Stretch=�6"   $��Stretch=�6>   $	��2,5q�6\   .+�5+      �� 
�������$��0q�6'   $	��5,2q�63   $��Image��6@   6R   ]� 5,      $��Stretch=�6   .+�5-      �� -   5        $��Right=�5-      $��6,0,2,0q�60   $	��8,0q�6A   ��6O   Z� ��5.      �� 5/      $% Resources/Images/cross-icon.png��6%   $ Uniform=�6S   51      �� 52      -   5        $ Image��52      $��2q�6u   $
��Left=�6�   ��6&   �� *Playback_Item_Temp.Image+$ PropertyChanged=�6�   53      54      �� 56   
   -   5        $��3��56      $��Right=�6   $��Center=�6:   $
��10,1q�6U   $��0,6,10,0q�6d   $
��Save��6v   
�������6�   57      58      