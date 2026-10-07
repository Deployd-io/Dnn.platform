var logger = (typeof logger !== 'undefined' && logger) || console;
// ********************
// Begin Popup Calendar
// ********************
var popCalDstFld;
var temp;
var popCalWin;

// ******************************
// Expected params:
// [0] Window Name
// [1] Destination Field
// [2] Short Date Format
// [3] Month Names
// [4] Day Names
// [5] Localized Today string
// [6] Localized Close string
// [7] Window Title
// [8] First Day of Week
// ******************************
function popupCal()
{
	logger.error('>>> Entering popupCal()', { "event.action": "popupCal" });
	//debugger;			//remove slashes to activate debugging in Visual Studio
	var tmpDate         = new Date();
	logger.error(`popupCal(): tmpDate → ${tmpDate}`, { "event.action": "popupCal" });
	var tmpString       = "";
	logger.error(`popupCal(): tmpString → ${tmpString}`, { "event.action": "popupCal" });
	var tmpNum          = 0;
	logger.error(`popupCal(): tmpNum → ${tmpNum}`, { "event.action": "popupCal" });
	var popCalDateVal;
	var dstWindowName   = "";

	//Initialize the window to an empty object.
	popCalWin = new Object();
	
	//Check for the right number of arguments
	if (arguments.length < 2)
	{
		alert("popupCal(): Wrong number of arguments.");
		return void(0);
	}
	//Get the command line arguments -- Localization is optional
	dstWindowName = popupCal.arguments[0];
	popCalDstFld = popupCal.arguments[1];
	temp = popupCal.arguments[1];
	popCalDstFmt = popupCal.arguments[2]; //Localized Short Date Format String
	popCalMonths = popupCal.arguments[3]; //Localized Month Names String
	popCalDays = popupCal.arguments[4];   //Localized Day Names String
	popCalToday = popupCal.arguments[5];  //Localized Today string
	popCalClose = popupCal.arguments[6];  //Localized Close string
	popCalTitle = popupCal.arguments[7];  //Window Title
	popCalFirstDayWeek = popupCal.arguments[8];  //First Day of Week
	
	//check destination field name
	if (popCalDstFld != "")
	  popCalDstFld = document.getElementById(popCalDstFld);

	//default localized short date format if not provided
	if (popCalDstFmt == "")
	  popCalDstFmt = "m/d/yyyy";

	//default localized months string if not provided
	if (popCalMonths == "")
	  popCalMonths = "January,February,March,April,May,June,July,August,September,October,November,December";
 
	//default localized months string if not provided
	if (popCalDays == "")
	  popCalDays = "Sun,Mon,Tue,Wed,Thu,Fri,Sat";
	
	//default localized today string if not provided
	if (popCalToday == "" || typeof popCalToday == "undefined")
	  popCalToday = "Today";

	//default localized close string if not provided
	if (popCalClose == "" || typeof popCalClose == "undefined")
	  popCalClose = "Close";

	//default window title if not provided
	if (popCalTitle == "" || typeof popCalTitle == "undefined")
	  popCalTitle = "Calendar";
 
	tmpString = new String(popCalDstFld.value);  
	//If tmpString is empty (meaning that the field is empty) 
	//use todays date as the starting point
	if(tmpString == "")
		popCalDateVal = new Date()
	else
	{
		//Make sure the century is included, if it isn't, add this 
		//century to the value that was in the field
		tmpNum = tmpString.lastIndexOf( "/" );
		if ( (tmpString.length - tmpNum) == 3 )
		{
			tmpString = tmpString.substring(0,tmpNum + 1)+"20"+tmpString.substr(tmpNum+1);
			popCalDateVal = new Date(tmpString);
		}
		else
		{
			//localized date support:
			//If we got to this point, it means the field that was passed 
			//in has no slashes in it. Use an extra function to build the date 
			//according to supplied date formatstring.
			popCalDateVal = getDateFromFormat(tmpString,popCalDstFmt);
		}
	}
	
	//Make sure the date is a valid date.  Set it to today if it is invalid
	//"NaN" is the return value for an invalid date
	if( popCalDateVal.toString() == "NaN" || popCalDateVal.toString() == "0")
	{
		popCalDateVal = new Date();
		popCalDstFld.value = "";
		logger.error(`popupCal(): popCalDateVal → ${popCalDateVal}`, { "event.action": "popupCal" });
	}
			
			logger.debug(`popupCal(): popCalDateVal → ${popCalDateVal}`, { "event.action": "popupCal" });
	//Set the base date to midnight of the first day of the specified month, 
	//this makes things easier?
 	var dateString = String(popCalDateVal.getMonth()+1) + "/" + String(popCalDateVal.getDate()) + "/" + String(popCalDateVal.getFullYear());
	logger.error(`popupCal(): dateString → ${dateString}`, { "event.action": "popupCal" });

	//Call the routine to draw the initial calendar
	reloadCalPopup(dateString, dstWindowName);
	
	logger.warn('<<< Exiting popupCal()', { "event.action": "popupCal" });
	return void(0);
}
 
function closeCalPopup()
{
	logger.error('>>> Entering closeCalPopup()', { "event.action": "closeCalPopup" });
	//Can't tell the child window to close itself, the parent window has to 
	//tell it to close.
	popCalWin.close();
	logger.warn('<<< Exiting closeCalPopup()', { "event.action": "closeCalPopup" });
	return void(0);
}
 
function reloadCalPopup() //[0]dateString, [1]dstWindowName
{
	logger.error('>>> Entering reloadCalPopup()', { "event.action": "reloadCalPopup" });
	//Set the window's features here

	var windowFeatures = "toolbar=no, location=no, status=no, menubar=no, scrollbars=no, resizable=no, height=270, width=270, top=" + ((screen.height - 270)/2).toString()+",left="+((screen.width - 270)/2).toString();
	logger.error(`reloadCalPopup(): windowFeatures → ${windowFeatures}`, { "event.action": "reloadCalPopup" });
	var tmpDate = new Date( reloadCalPopup.arguments[0] );
	logger.error(`reloadCalPopup(): tmpDate → ${tmpDate}`, { "event.action": "reloadCalPopup" });
	
	if (tmpDate.toString() == "Invalid Date")
	    tmpDate = new Date();
	
	tmpDate.setDate(1);
	
	//Get the calendar data
	var popCalData = calPopupSetData(tmpDate,reloadCalPopup.arguments[1]);
	logger.error(`reloadCalPopup(): popCalData → ${popCalData}`, { "event.action": "reloadCalPopup" });
 
	//Check to see if the window has been initialized, create it if it hasn't been
	if( popCalWin.toString() == "[object Object]" )
	{
		popCalWin = window.open("",reloadCalPopup.arguments[1],windowFeatures);
		popCalWin.opener = self;
		// Window im Vordergrund
		popCalWin.focus();
	}
	else 
	{
        popCalWin.document.close();
		popCalWin.document.write('');
    }
	
	//this is the line with the big problem
    popCalWin.document.write(popCalData);
	logger.warn('<<< Exiting reloadCalPopup()', { "event.action": "reloadCalPopup" });
    return void(1);
}
 
function calPopupSetData(firstDay,dstWindowName)
{
	logger.error(`>>> Entering calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName})`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var popCalData = "";
    var lastDate = 0;
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): popCalData → ${popCalData}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): lastDate → ${lastDate}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var fnt = new Array( "<FONT SIZE=\"1\">", "<B><FONT SIZE=\"2\">", "<FONT SIZE=\"2\" COLOR=\"#EF741D\"><B>");
	var dtToday = new Date();
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): fnt → ${fnt}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): dtToday → ${dtToday}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var thisMonth = firstDay.getMonth();
	var thisYear = firstDay.getFullYear();
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): thisMonth → ${thisMonth}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): thisYear → ${thisYear}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var nPrevMonth = (thisMonth == 0 ) ? 11 : (thisMonth - 1);
	var nNextMonth = (thisMonth == 11 ) ? 0 : (thisMonth + 1);
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): nPrevMonth → ${nPrevMonth}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): nNextMonth → ${nNextMonth}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var nPrevMonthYear = (nPrevMonth == 11) ? (thisYear - 1): thisYear;
	var nNextMonthYear = (nNextMonth == 0) ? (thisYear + 1): thisYear;
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): nPrevMonthYear → ${nPrevMonthYear}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): nNextMonthYear → ${nNextMonthYear}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var sToday = String((dtToday.getMonth()+1) + "/01/" + dtToday.getFullYear());
	var sPrevMonth = String((nPrevMonth+1) + "/01/" + nPrevMonthYear);
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): sToday → ${sToday}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): sPrevMonth → ${sPrevMonth}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var sNextMonth = String((nNextMonth+1) + "/01/" + nNextMonthYear);
	var sPrevYear1 = String((thisMonth+1) + "/01/" + (thisYear - 1));
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): sNextMonth → ${sNextMonth}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): sPrevYear1 → ${sPrevYear1}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var sNextYear1 = String((thisMonth+1) + "/01/" + (thisYear + 1));
 	var tmpDate = new Date( sNextMonth );
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): sNextYear1 → ${sNextYear1}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	
	tmpDate = new Date( tmpDate.valueOf() - 1001 );
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): tmpDate → ${tmpDate}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	lastDate = tmpDate.getDate();

	if (this.popCalMonths.split) // javascript 1.1 defensive code
	{
		var monthNames = this.popCalMonths.split(",");
		var dayNames = this.popCalDays.split(",");
	}
	else  // Need to build a js 1.0 split algorithm, default English for now
	{
		var monthNames = new Array("January","February","March","April","May","June","July","August","September","October","November","December");
		logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): monthNames → ${monthNames}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
		var dayNames = new Array("Sun","Mon","Tue","Wed","Thu","Fri","Sat")
		logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): dayNames → ${dayNames}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	}

 	var styles = "<style><!-- body{font-family:Arial,Helvetica,sans-serif;font-size:9pt}; td {  font-family: Arial, Helvetica, sans-serif; font-size: 9pt; color: #666666}; A { text-decoration: none; };TD.day { border-bottom: solid black; border-width: 0px; }--></style>"
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): styles → ${styles}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var cellAttribs = "align=\"center\" class=\"day\" BGCOLOR=\"#F1F1F1\"onMouseOver=\"temp=this.style.backgroundColor;this.style.backgroundColor='#CCCCCC';\" onMouseOut=\"this.style.backgroundColor=temp;\""
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): cellAttribs → ${cellAttribs}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var cellAttribs2 = "align=\"center\" BGCOLOR=\"#F1F1F1\" onMouseOver=\"temp=this.style.backgroundColor;this.style.backgroundColor='#CCCCCC';\" onMouseOut=\"this.style.backgroundColor=temp;\""
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): cellAttribs2 → ${cellAttribs2}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var htmlHead = "<HTML><HEAD><TITLE>"+popCalTitle+"</TITLE>" + styles + "</HEAD><BODY BGCOLOR=\"#F1F1F1\" TEXT=\"#000000\" LINK=\"#364180\" ALINK=\"#FF8100\" VLINK=\"#424282\">";
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): htmlHead → ${htmlHead}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var htmlTail = "</BODY></HTML>";
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): htmlTail → ${htmlTail}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var closeAnchor = "<CENTER><input type=button value=\""+popCalClose+"\" onClick=\"javascript:window.opener.closeCalPopup()\"></CENTER>";            
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): closeAnchor → ${closeAnchor}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var todayAnchor = "<A HREF=\"javascript:window.opener.reloadCalPopup('"+sToday+"','"+dstWindowName+"');\">"+popCalToday+"</A>";
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): todayAnchor → ${todayAnchor}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var prevMonthAnchor = "<A HREF=\"javascript:window.opener.reloadCalPopup('"+sPrevMonth+"','"+dstWindowName+"');\">" + monthNames[nPrevMonth] + "</A>";
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): prevMonthAnchor → ${prevMonthAnchor}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var nextMonthAnchor = "<A HREF=\"javascript:window.opener.reloadCalPopup('"+sNextMonth+"','"+dstWindowName+"');\">" + monthNames[nNextMonth] + "</A>";
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): nextMonthAnchor → ${nextMonthAnchor}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var prevYear1Anchor = "<A HREF=\"javascript:window.opener.reloadCalPopup('"+sPrevYear1+"','"+dstWindowName+"');\">"+(thisYear-1)+"</A>";
	logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): prevYear1Anchor → ${prevYear1Anchor}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	var nextYear1Anchor = "<A HREF=\"javascript:window.opener.reloadCalPopup('"+sNextYear1+"','"+dstWindowName+"');\">"+(thisYear+1)+"</A>";
		logger.error(`calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName}): nextYear1Anchor → ${nextYear1Anchor}`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
		
	popCalData += (htmlHead + fnt[1]);
	popCalData += ("<DIV align=\"center\">");
	popCalData += ("<TABLE BORDER=\"0\" cellspacing=\"0\" callpadding=\"0\" width=\"250\"><TR><TD width=\"45\">&nbsp</TD>");
	popCalData += ("<TD width=\"45\" align=\"center\" " + cellAttribs2);
	popCalData += (" >");
	popCalData += (fnt[0]+prevYear1Anchor+"</FONT></TD>");
	popCalData += ("<TD width=\"70\" align=\"center\" "+cellAttribs2);
	popCalData += (" >");
	popCalData += (fnt[0]+todayAnchor+"</FONT></TD>");
	popCalData += ("<TD width=\"45\" align=\"center\" "+cellAttribs2);
	popCalData += (" >");
	popCalData += (fnt[0]+nextYear1Anchor+"</FONT></TD><TD width=\"45\">&nbsp</TD>");
	popCalData += ("</TR></TABLE>");
	popCalData += ("<TABLE BORDER=\"0\" cellspacing=\"0\" callpadding=\"0\" width=\"250\">");          
	popCalData += ("<TR><TD width=\"55\" align=\"center\" "+cellAttribs2);
	popCalData += (" >");
	popCalData += (fnt[0] + prevMonthAnchor + "</FONT></TD>");
	popCalData += ("<TD width=\"140\" align=\"center\">");
	popCalData += ("&nbsp;&nbsp;"+fnt[1]+"<FONT COLOR=\"#000000\">" + monthNames[thisMonth] + ", " + thisYear + "&nbsp;&nbsp;</FONT></TD>");
	popCalData += ("<TD width=\"55\" align=\"center\" "+cellAttribs2);
	popCalData += (" >");
	popCalData += (fnt[0]+nextMonthAnchor+"</FONT></TD></TR></TABLE><BR>");       
	popCalData += ("<TABLE BORDER=\"0\" cellspacing=\"2\" cellpadding=\"1\"  width=\"245\">" );
	popCalData += ("");
	popCalData += ("<TR>");
	
	/*
	popCalData += ("<TD width=\"35\" align=\"center\">"+fnt[1]+"<FONT COLOR=\"#000000\">"+dayNames[0]+"</FONT></TD>");
	popCalData += ("<TD width=\"35\" align=\"center\">"+fnt[1]+"<FONT COLOR=\"#000000\">"+dayNames[1]+"</FONT></TD>");
	popCalData += ("<TD width=\"35\" align=\"center\">"+fnt[1]+"<FONT COLOR=\"#000000\">"+dayNames[2]+"</FONT></TD>");
	popCalData += ("<TD width=\"35\" align=\"center\">"+fnt[1]+"<FONT COLOR=\"#000000\">"+dayNames[3]+"</FONT></TD>");
	popCalData += ("<TD width=\"35\" align=\"center\">"+fnt[1]+"<FONT COLOR=\"#000000\">"+dayNames[4]+"</FONT></TD>");
	popCalData += ("<TD width=\"35\" align=\"center\">"+fnt[1]+"<FONT COLOR=\"#000000\">"+dayNames[5]+"</FONT></TD>");
	popCalData += ("<TD width=\"35\" align=\"center\">"+fnt[1]+"<FONT COLOR=\"#000000\">"+dayNames[6]+"</FONT></TD>");
	*/
	var xday = 0;
	for (xday = 0; xday < 7; xday++)
	{
		popCalData += ("<TD width=\"35\" align=\"center\">"+fnt[1]+"<FONT COLOR=\"#000000\">"+dayNames[(xday+popCalFirstDayWeek)%7]+"</FONT></TD>");
	};
	popCalData += ("</TR>");
	
	var calDay = 0;
	var monthDate = 1;
	var weekDay = firstDay.getDay();
	do
	{
		popCalData += ("<TR>");
		for (calDay = 0; calDay < 7; calDay++ )
		{
			if(((weekDay+7-popCalFirstDayWeek)%7 != calDay) || (monthDate > lastDate))
			{
				popCalData += ("<TD width=\"35\">"+fnt[1]+"&nbsp;</FONT></TD>");
				continue;
			}
			else
			{
				anchorVal = "<A HREF=\"javascript:window.opener.calPopupSetDate(window.opener.popCalDstFld,'" + constructDate(monthDate,thisMonth+1,thisYear) + "');window.opener.closeCalPopup()\">";
				jsVal = "javascript:window.opener.calPopupSetDate(window.opener.popCalDstFld,'" + constructDate(monthDate,thisMonth+1,thisYear) + "');window.opener.closeCalPopup()";

				popCalData += ("<TD width=\"35\" "+cellAttribs+" onClick=\""+jsVal+"\">");
				
				if ((firstDay.getMonth() == dtToday.getMonth()) && (monthDate == dtToday.getDate()) && (thisYear == dtToday.getFullYear()) )
					popCalData += (anchorVal+fnt[2]+monthDate+"</A></FONT></TD>");
				else
					popCalData += (anchorVal+fnt[1]+monthDate+"</A></FONT></TD>");
				
				weekDay++;
				monthDate++;
			}
		}
		weekDay = popCalFirstDayWeek;
		popCalData += ("</TR>");
	} while( monthDate <= lastDate );
	
	popCalData += ("</TABLE></DIV><BR>");
 
	popCalData += (closeAnchor+"</FONT>"+htmlTail);
	logger.warn(`<<< Exiting calPopupSetData(firstDay=${firstDay},dstWindowName=${dstWindowName})`, { "event.action": "calPopupSetData", "logrite.firstDay": firstDay, "logrite.dstWindowName": dstWindowName });
	return( popCalData );
}
 
function calPopupSetDate()
{
	logger.error('>>> Entering calPopupSetDate()', { "event.action": "calPopupSetDate" });
	calPopupSetDate.arguments[0].value = calPopupSetDate.arguments[1];
	logger.warn('<<< Exiting calPopupSetDate()', { "event.action": "calPopupSetDate" });
}

// utility function
function padZero(num)
{
logger.error(`>>> Entering padZero(num=${num})`, { "event.action": "padZero", "logrite.num": num });
logger.warn(`<<< Exiting padZero(num=${num})`, { "event.action": "padZero", "logrite.num": num });
  return ((num <= 9) ? ("0" + num) : num);
}

// Format short date
function constructDate(d,m,y)
{
logger.error(`>>> Entering constructDate(d=${d},m=${m},y=${y})`, { "event.action": "constructDate", "logrite.d": d, "logrite.m": m, "logrite.y": y });
  var fmtDate = this.popCalDstFmt
  fmtDate = fmtDate.replace ('dd', padZero(d))
  fmtDate = fmtDate.replace ('d', d)
  fmtDate = fmtDate.replace ('MM', padZero(m))
  fmtDate = fmtDate.replace ('M', m)
  fmtDate = fmtDate.replace ('yyyy', y)
  fmtDate = fmtDate.replace ('yy', padZero(y%100))
logger.warn(`<<< Exiting constructDate(d=${d},m=${m},y=${y})`, { "event.action": "constructDate", "logrite.d": d, "logrite.m": m, "logrite.y": y });
  return fmtDate;
}

// ------------------------------------------------------------------
// Utility functions for parsing in getDateFromFormat()
// ------------------------------------------------------------------
function _isInteger(val) {
	logger.error(`>>> Entering _isInteger(val=${val})`, { "event.action": "_isInteger", "logrite.val": val });
	var digits="1234567890";
	for (var i=0; i < val.length; i++) {
		if (digits.indexOf(val.charAt(i))==-1) { return false; }
		}
	logger.error(`_isInteger(val=${val}): digits → ${digits}`, { "event.action": "_isInteger", "logrite.val": val });
	return true;
	}
function _getInt(str,i,minlength,maxlength) {
	logger.error(`>>> Entering _getInt(str=${str},i=${i},minlength=${minlength},maxlength=${maxlength})`, { "event.action": "_getInt", "logrite.str": str, "logrite.i": i, "logrite.minlength": minlength, "logrite.maxlength": maxlength });
	for (var x=maxlength; x>=minlength; x--) {
		var token=str.substring(i,i+x);
		if (token.length < minlength) { return null; }
		if (_isInteger(token)) { return token; }
		}
	logger.warn(`<<< Exiting _getInt(str=${str},i=${i},minlength=${minlength},maxlength=${maxlength})`, { "event.action": "_getInt", "logrite.str": str, "logrite.i": i, "logrite.minlength": minlength, "logrite.maxlength": maxlength });
	return null;
	}
	
// ------------------------------------------------------------------
// getDateFromFormat( date_string , format_string )
//
// This function takes a date string and a format string. It matches
// If the date string matches the format string, it returns the 
// getTime() of the date. If it does not match, it returns 0.
// ------------------------------------------------------------------
function getDateFromFormat(val,format) {
	logger.error(`>>> Entering getDateFromFormat(val=${val},format=${format})`, { "event.action": "getDateFromFormat", "logrite.val": val, "logrite.format": format });
	val=val+"";
	format=format+"";
	logger.error(`getDateFromFormat(val=${val},format=${format}): val → ${val}`, { "event.action": "getDateFromFormat", "logrite.val": val, "logrite.format": format });
	logger.error(`getDateFromFormat(val=${val},format=${format}): format → ${format}`, { "event.action": "getDateFromFormat", "logrite.val": val, "logrite.format": format });
	var i_val=0;
	var i_format=0;
	var c="";
	logger.error(`getDateFromFormat(val=${val},format=${format}): c → ${c}`, { "event.action": "getDateFromFormat", "logrite.val": val, "logrite.format": format });
	var token="";
	var x,y;
	var now=new Date();
	logger.error(`getDateFromFormat(val=${val},format=${format}): now → ${now}`, { "event.action": "getDateFromFormat", "logrite.val": val, "logrite.format": format });
	var year=now.getYear();
	logger.error(`getDateFromFormat(val=${val},format=${format}): year → ${year}`, { "event.action": "getDateFromFormat", "logrite.val": val, "logrite.format": format });
	var month=now.getMonth()+1;
	logger.error(`getDateFromFormat(val=${val},format=${format}): month → ${month}`, { "event.action": "getDateFromFormat", "logrite.val": val, "logrite.format": format });
	var date=1;
		
	while (i_format < format.length) {
		// Get next token from format string
		c=format.charAt(i_format);
		token="";
		while ((format.charAt(i_format)==c) && (i_format < format.length)) {
			token += format.charAt(i_format++);
			}
		// Extract contents of value based on format token
		if (token=="yyyy" || token=="yy" || token=="y") {
			if (token=="yyyy") { x=4;y=4; }
			if (token=="yy")   { x=2;y=2; }
			if (token=="y")    { x=2;y=4; }
			year=_getInt(val,i_val,x,y);
			if (year==null) { return 0; }
			i_val += year.length;
			if (year.length==2) {
				if (year > 70) { year=1900+(year-0); }
				else { year=2000+(year-0); }
				}
			}
		else if (token=="MM"||token=="M") {
			month=_getInt(val,i_val,token.length,2);
			if(month==null||(month<1)||(month>12)){return 0;}
			i_val+=month.length;}
		else if (token=="dd"||token=="d") {
			date=_getInt(val,i_val,token.length,2);
			if(date==null||(date<1)||(date>31)){return 0;}
			i_val+=date.length;}
		else {
			if (val.substring(i_val,i_val+token.length)!=token) {return 0;}
			else {i_val+=token.length;}
			}
		}
	// If there are any trailing characters left in the value, it doesn't match
	if (i_val != val.length) { return 0; }
	// Is date valid for month?
	if (month==2) {
		// Check for leap year
		if ( ( (year%4==0)&&(year%100 != 0) ) || (year%400==0) ) { // leap year
			if (date > 29){ return 0; }
			}
		else { if (date > 28) { return 0; } }
		}
	if ((month==4)||(month==6)||(month==9)||(month==11)) {
		if (date > 30) { return 0; }
		}
	var newdate=new Date(year,month-1,date);
	logger.warn(`<<< Exiting getDateFromFormat(val=${val},format=${format})`, { "event.action": "getDateFromFormat", "logrite.val": val, "logrite.format": format });
	return newdate;
	}

if (typeof(Sys) != "undefined"){
    Sys.Application.notifyScriptLoaded() ;
}

// ******************
// End Popup Calendar
// ******************
