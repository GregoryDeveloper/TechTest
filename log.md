# Log

## Asummptions
The currency is £ for now. If extra currencies become available the validator class would need to be extended.
ProductOne is only available to people aged 18 to 39. I assumed 18 and 39 were included.
ProductTwo is only available to people aged 18 to 50. I assumed 18 and 50 were included.

## Decisions
Comments were left in the code to explain some decisions.
I have implemented IDateTimeProvider to make the code testable.
The domain event was extended to provide an error event. It is not a breaking change.
A test project was created to test the project. The code is not fully covered, that would be an improvement.


## Observations
There are missing information from the application object to fully map the object

## Todo
More unit tests could be added.
A retry policy could be added if there are api calls.
Mapping should be in its own separated object for a better separation of concern.
Services could be created to use the administration services but given the low amount of code, I decided it was not needed at the time.
The validation class could be updated to return a result object rather than a bool. A more specific error could be sent in that way.