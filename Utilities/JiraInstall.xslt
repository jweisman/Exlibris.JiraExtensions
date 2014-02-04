<?xml version="1.0"?>

<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:template match="/">
    <items>
      <xsl:for-each select="rss/channel/item">
        <item>
          <key>
            <xsl:value-of select="key"/>
          </key>
          <summary>
            <xsl:value-of select="summary"/>
          </summary>
          <link>
            <xsl:value-of select="link"/>
          </link>
          <username>
            <xsl:value-of select="assignee/@username"/>
          </username>
          <installer>
            <xsl:value-of select="assignee"/>
          </installer>
          <product>
            <xsl:value-of select="component"/>
          </product>
          <startdate>
            <xsl:value-of select="customfields/customfield[@id='customfield_10035']/customfieldvalues/customfieldvalue"/>
          </startdate>
          <duration>
            <xsl:value-of select="customfields/customfield[@id='customfield_10036']/customfieldvalues/customfieldvalue"/>
          </duration>
          <installtask>
            <xsl:value-of select="customfields/customfield[@id='customfield_10033']/customfieldvalues/customfieldvalue"/>
          </installtask>
          <customercode>
            <xsl:value-of select="summary"/>
          </customercode>
          <dateapproved>
            <xsl:value-of select="customfields/customfield[@id='customfield_10050']/customfieldvalues/customfieldvalue"/>
          </dateapproved>
          <si>
            <xsl:value-of select="customfields/customfield[@id='customfield_10021']/customfieldvalues/customfieldvalue"/>
          </si>
          <region>
            <xsl:value-of select="customfields/customfield[@id='customfield_10070']/customfieldvalues/customfieldvalue"/>
          </region>
            <hosted>
                <xsl:value-of select="customfields/customfield[@id='customfield_10140']/customfieldvalues/customfieldvalue"/>
            </hosted>
            <ryo>
                <xsl:value-of select="customfields/customfield[@id='customfield_10091']/customfieldvalues/customfieldvalue"/>
            </ryo>
            <security>
                <xsl:value-of select="customfields/customfield[@id='customfield_10060']/customfieldvalues/customfieldvalue"/>
            </security>
        </item>
      </xsl:for-each>
    </items>

  </xsl:template>

</xsl:stylesheet>