# Language File Format
Each translation file contains ids followed by their translation:
```
sample.text=This is a sample text
sample.body=Body of a sample text\n\t
```

Comments can be added by writing `###` at the end of a line. The first instance of `###` in the line will be the start of the comment
```py
sample.comment=This would be a comment ### Comment
sample.test=### test ###
```

If any translation string is empty the app will use the fallback translation.

Trailing whitespaces after a string will be removed by the app.

## Adding a language
There is one template file `en_US/en_US.lang` that can be used as a guide when adding a new language. This file will always be updated with the latest ids.

To add a new language you need to modify the file `languages.json` and add a new field for the language.

```json
{
	"en_US": "English",
	"sv_SE": "Swedish"
	/* Add more languages here */
}
```

If you want to see were ids are used you can add the `debug` language.

```json
{
	"debug": "Debug",
	"en_US": "English"
}
```

## Debugging
If anything has been wrongly formatted the Logger will contain information about what went wrong. Always try out the final version of the language you are adding before making any PR's

## Credits
Anyone that helps me translate the app will be credited in both their translation file and in the project `README.md`
