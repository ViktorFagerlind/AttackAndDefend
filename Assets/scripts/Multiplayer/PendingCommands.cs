using System;
using System.Collections.Generic;

public class PendingCommands
{
  private static readonly GameLogger logger = GameLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

  // -------------------------------------------------------------------------------------------------------------------

  private static readonly int m_nofTurns = 5;

  // i=0: current
  // i=1: next
  // i=2: next next 
  // i=3: next next next 
  // i=4: next next next next 
  private Command[][]         m_commands;
  private int                 m_nofPlayers;
  private int[]               m_commandCounts;

  // -------------------------------------------------------------------------------------------------------------------
  
  public PendingCommands (int nofPlayers)
  {
    m_nofPlayers = nofPlayers;
    
    m_commands    = new Command[m_nofTurns][];
    for (int i=0; i<m_commands.Length; i++) 
      m_commands[i] = new Command[nofPlayers];

    m_commandCounts = new int[m_nofTurns];
  }
  
  public void NewTurn ()
  {
    // Finished processing this turns commands - clear it
    for (int i=0; i<m_commands[0].Length; i++)
      m_commands[0][i] = null;

    // Shift all commands and counts
    Command[] swap = m_commands[0];

    for (int i=0; i<m_commands.Length-1; i++)
      m_commands[i] = m_commands[i+1];

    m_commands[m_commands.Length-1] = swap;

    for (int i=0; i<m_commandCounts.Length-1; i++)
      m_commandCounts[i] = m_commandCounts[i+1];

    m_commandCounts[m_commandCounts.Length-1] = 0;
  }
  
  public void AddCommand (Command command, int playerID, int lockStepTurn, int commandsLockStepTurn)
  {
    logger.Debug ("Player " + playerID + " added pending command for turn " + commandsLockStepTurn);

    int index = commandsLockStepTurn - lockStepTurn + 2;

    if ((index <= 0) || (index > (m_nofTurns - 1)))
    {
      logger.Error ("WARNING!!!! Unexpected lockstepID " + commandsLockStepTurn + " received"); //TODO: Error Handling
      return;
    }

    // if command is for next turn, add for processing 3 turns away
    if (m_commands[index][playerID-1] != null)
      logger.Error ("WARNING!!!! Recieved multiple commands from player " + playerID + " for turn " + commandsLockStepTurn); //TODO: Error Handling
    
    m_commands[index][playerID-1] = command;
    m_commandCounts[index]++;

    return;
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  public bool ReadyForNextTurn (int lockstepTurn)
  {
    // First command is sent in turn 1, therefore no check is needed until turn 2 (pending 3)
    if (lockstepTurn <= 1)
      return true;

    // Check that all Commands that will be processed next turn have been recieved
    return m_commandCounts[1] == m_nofPlayers;
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public void ExecuteCurrentCommands ()
  {
    foreach (Command command in m_commands[0])
      command.Execute ();
  }

}