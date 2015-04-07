using System;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmedCommands
{
  private static readonly GameLogger logger = GameLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
  
  // -------------------------------------------------------------------------------------------------------------------

  private List<PhotonPlayer>   m_playersConfirmedCurrentCommand;
  private List<PhotonPlayer>   m_playersConfirmedPriorCommand;

  private int                  m_nofPlayers;

  // -------------------------------------------------------------------------------------------------------------------
  
  public ConfirmedCommands (int nofPlayers)
  {
    m_nofPlayers = nofPlayers;

    m_playersConfirmedCurrentCommand = new List<PhotonPlayer> (nofPlayers);
    m_playersConfirmedPriorCommand   = new List<PhotonPlayer> (nofPlayers);
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public void AddCommand (PhotonPlayer player, int lockStepTurn, int commandsLockStepTurn)
  {
    if (commandsLockStepTurn == lockStepTurn)
      m_playersConfirmedCurrentCommand.Add (player);
    else if (commandsLockStepTurn == lockStepTurn - 1)
      m_playersConfirmedPriorCommand.Add (player);
    else
    {
      //TODO: Error Handling
      logger.Debug("WARNING!!!! Unexpected lockstepID Confirmed : " + lockStepTurn + " from player: " + player.ToString());
    }
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public void NewTurn ()
  {
    // clear prior actions
    m_playersConfirmedPriorCommand.Clear();
    
    List<PhotonPlayer> swap = m_playersConfirmedPriorCommand;
    
    // last turns actions is now this turns prior actions
    m_playersConfirmedPriorCommand = m_playersConfirmedCurrentCommand;
    
    // set this turns confirmation actions to the empty list
    m_playersConfirmedCurrentCommand = swap;
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  public bool ReadyForNextTurn (int lockstepTurn)
  {
    // First command is sent in turn 1, therefore no check is needed until turn 2 (pending 3)
    if (lockstepTurn <= 1)
      return true;

    // check that the action that is going to be processed has been confirmed
    return m_playersConfirmedPriorCommand.Count == m_nofPlayers;
  }

  // -------------------------------------------------------------------------------------------------------------------
}