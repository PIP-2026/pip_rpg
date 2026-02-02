using UnityEngine;
/// <remarks>
///   <para>
///     Author: Christof Kloninger / <a href="mailto:gme.24.kloninger@gmail.com">gme.24.kloninger@gmail.com</a>
///   </para>
///   <para>
///       Issue: <a href="">link to issue</a>
///   </para>
///   <para>
///     This class is to handle the quit of the Application only and a failsafe to whenever an unexpected event forces the application to quit.
///     For example a player who would want to quit the application. 
///   </para>
/// </remarks>
/// <summary>
///   The Quit Application method should be called only in the most extreme of cases. 
/// </summary>

public class QuitTheApplication : MonoBehaviour
{
	public void QuitApplication()
	{
    // Save the profile. Once the Save Manager or whatever is taking care of that function. 
    // We can think of removing all Listeners to the events just to make sure.
        Application.Quit();
	}
}
