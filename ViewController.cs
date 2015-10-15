using System;

using UIKit;
using LocalAuthentication;
using Foundation;

namespace DotNetMiami
{
	public partial class ViewController : UIViewController
	{
		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}


		public override bool ShouldPerformSegue (string segueIdentifier, NSObject sender)
		{
			if (segueIdentifier == "SegueToTest") {
				return  false; // Return false, let btnLogin handle Segue
			}

			return true;
		}

		partial void btnLogin_TouchUpInside (UIButton sender)
		{
			NSError _error;

			using (var _context = new LAContext ()) {

				// Allows us to reuse Touch ID verificaton from unlocking device up to 30 seconds later
				_context.TouchIdAuthenticationAllowableReuseDuration = 30;

				if (_context.CanEvaluatePolicy (LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out _error)) {

					// Get enrollment state
					var policyState = _context.EvaluatedPolicyDomainState.ToString().Replace("<", "").Replace(">", "").Replace(" ", "");

					if (TouchIDPolicyDomainState != null)
					{
						if (policyState != TouchIDPolicyDomainState)
							Console.WriteLine("Fingerprints enrollments changed.");
						else
							Console.WriteLine("Fingerprints enrollments remain the same.");
					}

					// Store enrollment
					TouchIDPolicyDomainState = policyState;

					var replyHandler = new LAContextReplyHandler ((success, error) => {
						InvokeOnMainThread (() => {

							if (success) {
								PerformSegue("SegueToTest", null);
							} else {
								DisplayAlertOKPopup("", "Unable to authenticate.");
							}
						});
					});

					// Add reason why we are using Touch ID
					_context.EvaluatePolicy (LAPolicy.DeviceOwnerAuthenticationWithBiometrics, "Login as user: X", replyHandler);
				} else {
					DisplayAlertOKPopup("", "Touch ID not available on this device.");
				}
			}
		}

		public void DisplayAlertOKPopup(string title, string message)
		{
			var act = UIAlertAction.Create ("OK", UIAlertActionStyle.Default, null);

			using (var alertCntrl = UIAlertController.Create (title, message, UIAlertControllerStyle.Alert)) {

				alertCntrl.AddAction (act);

				this.PresentViewController(alertCntrl, true, null);
			}
		}

		public static string TouchIDPolicyDomainState
		{
			get { 
				return NSUserDefaults.StandardUserDefaults.StringForKey("TouchIDPolicyDomainState"); 
			}
			set {
				NSUserDefaults.StandardUserDefaults.SetString(value, "TouchIDPolicyDomainState"); 
				NSUserDefaults.StandardUserDefaults.Synchronize ();
			}
		}
	}
}

