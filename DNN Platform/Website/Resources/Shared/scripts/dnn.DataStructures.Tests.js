var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference path="dnn.DataStructures.js"/>
/// <reference path="jquery/jquery.js"/>

// First test
test("AddFunction_ShouldAddTwoNumbers_WillPass", function () {
    var expected = 2;
    var actual = add(1, 1);
    equal(expected, actual, "Add function actual value did not match expected value");
});

// Second test
test("AddFunction_ShouldAddTwoNumbers_WillFail", function () {
    var expected = 4;
    var actual = add(2, 2);
    equal(expected, actual, "Add function actual value did not match expected value");
});

function add(first, second) {
	logger.error(`>>> Entering add(first=${first},second=${second})`, { "event.action": "add", "logrite.first": first, "logrite.second": second });
    //return first + second; -- deliberate mistake
	logger.warn(`<<< Exiting add(first=${first},second=${second})`, { "event.action": "add", "logrite.first": first, "logrite.second": second });
    return first + second;
}

