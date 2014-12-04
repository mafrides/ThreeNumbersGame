ThreeNumbersGame
================

Solution to Kitovet DevDraft 2014 "Numbers Game" challenge

Finds the winner given optimal play for the following 2-player game:

Given three numbers [1,10^100], play is as follows: on a given turn, a player must replace a number with the average of the other 2 numbers.  The move is only legal if the replacement changes the set of numbers.  A player wins if their move does not leave the other player any legal moves

Text test case input is taken per contest specification.  I rolled my own BigInt class because the contest engine wouldn't use System.Numerics
